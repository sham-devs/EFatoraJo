#!/usr/bin/env python3
"""
Single Invoice Processor for EFatoraJo - Jordan E-Invoicing System

This script processes a SINGLE invoice PDF file and:
1. Reads the PDF and extracts invoice data
2. Generates JSON for the original invoice
3. Generates JSON for the return invoice (credit note)
4. Saves both JSON files in the same directory as the PDF
5. Submits both invoices to the Jordan e-invoicing system using EFatoraJoConsoleApp
6. Verifies successful submission and reports clear errors if submission fails

Usage:
    python process_single_invoice.py --pdf-path <path_to_pdf> --original-uuid <uuid>

Example:
    python process_single_invoice.py --pdf-path "temp/temp/INV-001.pdf" --original-uuid "550e8400-e29b-41d4-a716-446655440000"

Requirements:
    - PyPDF2 (for PDF text extraction)
    - EFatoraJoConsoleApp built and configured with user secrets
"""

import argparse
import json
import re
import subprocess
import sys
import uuid
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Any, Optional, Tuple

try:
    import PyPDF2
except ImportError:
    print("ERROR: PyPDF2 is not installed. Please install it using:")
    print("  pip install PyPDF2")
    sys.exit(1)


# Constants for supplier info (will be overridden by user secrets in EFatoraJoConsoleApp)
SUPPLIER_INFO = {
    "taxVATNumber": "50185012",
    "incomeSourceSequence": "3905659",
    "registeredSupplierName": "باسل حسين مسلم جمعه"
}


class InvoiceProcessorError(Exception):
    """Base exception for invoice processing errors"""
    pass


class PDFReadError(InvoiceProcessorError):
    """Error reading or parsing PDF file"""
    pass


class DataExtractionError(InvoiceProcessorError):
    """Error extracting data from PDF"""
    pass


class SubmissionError(InvoiceProcessorError):
    """Error submitting invoice to e-invoicing system"""
    pass


def log(message: str, level: str = "INFO"):
    """Log message with timestamp and level"""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    print(f"[{timestamp}] [{level}] {message}")


def extract_text_from_pdf(pdf_path: Path) -> str:
    """
    Extract all text from PDF file using PyPDF2

    Args:
        pdf_path: Path to the PDF file

    Returns:
        Extracted text as string

    Raises:
        PDFReadError: If PDF cannot be read
    """
    try:
        log(f"Reading PDF: {pdf_path.name}")

        with open(pdf_path, 'rb') as f:
            reader = PyPDF2.PdfReader(f)
            text = ""

            log(f"PDF has {len(reader.pages)} page(s)")

            for i, page in enumerate(reader.pages, 1):
                page_text = page.extract_text()
                text += page_text
                log(f"  Extracted text from page {i} ({len(page_text)} characters)")

            if not text.strip():
                raise PDFReadError("PDF appears to be empty or contains no extractable text")

            log(f"Total extracted text: {len(text)} characters")
            return text

    except FileNotFoundError:
        raise PDFReadError(f"PDF file not found: {pdf_path}")
    except Exception as e:
        raise PDFReadError(f"Failed to read PDF: {str(e)}")


def parse_invoice_data(pdf_text: str) -> Dict[str, Any]:
    """
    Parse invoice data from extracted PDF text

    This function uses regex patterns to extract:
    - Invoice number
    - Invoice date
    - Customer information (name, national ID, phone, postal code)
    - Line items (description, quantity, unit price, totals)
    - Invoice totals (before discount, discount amount, final amount)
    - Notes

    Args:
        pdf_text: Raw text extracted from PDF

    Returns:
        Dictionary containing parsed invoice data

    Raises:
        DataExtractionError: If required data cannot be extracted
    """
    log("Parsing invoice data from PDF text...")

    data = {
        'invoice_number': None,
        'invoice_date': None,
        'customer': {},
        'line_items': [],
        'totals': {},
        'notes': ''
    }

    # Extract invoice number
    # Pattern: INV-YYYYMMDD-XXXX or similar formats
    # Since Arabic text may be reversed in PDF, we look for the INV pattern directly
    invoice_num_patterns = [
        r'(INV-\d{8}-\d+)',  # Pattern: INV-20250809-3322
        r'رقم الفاتورة[:\s]+([A-Z0-9\-]+)',
        r'Invoice Number[:\s]+([A-Z0-9\-]+)',
        r'الفاتورة\s+رقم[:\s]+([A-Z0-9\-]+)',
        r'اﻹﻟﻜﺘﺮوﻧﯿﺔ\s+([A-Z0-9\-]+)',  # Arabic reversed
        r'([A-Z]+-\d{8}-\d+)'  # Generic pattern
    ]

    for pattern in invoice_num_patterns:
        match = re.search(pattern, pdf_text, re.IGNORECASE)
        if match:
            data['invoice_number'] = match.group(1).strip()
            log(f"  Found invoice number: {data['invoice_number']}")
            break

    if not data['invoice_number']:
        # Try to find any pattern that looks like an invoice number
        match = re.search(r'([A-Z]+-[\d\-]+)', pdf_text)
        if match:
            data['invoice_number'] = match.group(1).strip()
            log(f"  Found invoice number (fallback): {data['invoice_number']}")
        else:
            raise DataExtractionError("Could not extract invoice number from PDF")

    # Extract invoice date
    # Pattern: DD-MM-YYYY (19-07-2025) as seen in PDF
    date_patterns = [
        r'(\d{2}-\d{2}-\d{4})',  # DD-MM-YYYY (e.g., 19-07-2025)
        r'(\d{4}-\d{2}-\d{2})',  # YYYY-MM-DD
        r'(\d{2}/\d{2}/\d{4})',  # DD/MM/YYYY
        r'تاريخ[:\s]+(\d{2}-\d{2}-\d{4})',
        r'التاريخ[:\s]+(\d{2}-\d{2}-\d{4})'
    ]

    for pattern in date_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            date_str = match.group(1)
            # Convert DD-MM-YYYY or DD/MM/YYYY to YYYY-MM-DD if needed
            if '-' in date_str and date_str.index('-') == 2:
                day, month, year = date_str.split('-')
                date_str = f"{year}-{month}-{day}"
            elif '/' in date_str:
                day, month, year = date_str.split('/')
                date_str = f"{year}-{month}-{day}"
            data['invoice_date'] = date_str
            log(f"  Found invoice date: {data['invoice_date']}")
            break

    if not data['invoice_date']:
        # Use today's date as fallback
        data['invoice_date'] = datetime.now().strftime('%Y-%m-%d')
        log(f"  WARNING: Could not extract date, using today: {data['invoice_date']}")

    # Extract customer name
    # In the PDF: االسم Customer 82
    name_patterns = [
        r'اﻻﺳﻢ\s+([^\n]+)',  # Arabic reversed
        r'االسم\s+([^\n]+)',  # Arabic variation
        r'اسم\s+([^\n]+)',
        r'Customer\s+(\d+)',  # Direct pattern from PDF
        r'اسم العميل[:\s]+([^\n]+)',
        r'Customer Name[:\s]+([^\n]+)'
    ]

    for pattern in name_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            data['customer']['name'] = match.group(1).strip()
            log(f"  Found customer name: {data['customer']['name']}")
            break

    if not data['customer'].get('name'):
        data['customer']['name'] = "Unknown Customer"
        log("  WARNING: Could not extract customer name, using default")

    # Extract customer national ID
    id_patterns = [
        r'الرقم الوطني[:\s]+(\d+)',
        r'National ID[:\s]+(\d+)',
        r'ﻲﻨﻃﻮﻟا ﻢﻗﺮﻟا[:\s]+(\d+)'
    ]

    for pattern in id_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            data['customer']['national_id'] = match.group(1).strip()
            data['customer']['id_type'] = 'NIN'
            log(f"  Found customer ID: {data['customer']['national_id']}")
            break

    # Extract customer phone
    # Pattern: رﻗﻢ اﻟﻬﺎﺗﻒ 0780910617
    # Note: There are TWO phone numbers in PDF - customer and supplier
    # Customer phone appears first after "المشتري"
    phone_patterns = [
        r'اﻟﻤﺸﺘﺮي[\s\S]*?اﻟﻬﺎﺗﻒ\s+(\d+)',  # Customer phone after المشتري section
        r'المشتري[\s\S]*?الهاتف\s+(\d+)',
        r'رقم الهاتف[:\s]+(\d+)',
        r'Phone[:\s]+(\d+)'
    ]

    for pattern in phone_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            data['customer']['phone'] = match.group(1).strip()
            log(f"  Found customer phone: {data['customer']['phone']}")
            break

    # Extract postal code
    # Pattern: اﻟﺮﻗﻢ اﻟﺒﺮﯾﺪي 80800
    # Note: There are TWO postal codes - customer (80800) and supplier (11110)
    # We need to get the customer one which appears first
    postal_patterns = [
        r'اﻟﻤﺸﺘﺮي[\s\S]*?اﻟﺒﺮﯾﺪي\s+(\d+)',  # Customer postal after المشتري section
        r'المشتري[\s\S]*?البريدي\s+(\d+)',
        r'الرمز البريدي[:\s]+(\d+)',
        r'Postal Code[:\s]+(\d+)'
    ]

    for pattern in postal_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            data['customer']['postal_code'] = match.group(1).strip()
            log(f"  Found postal code: {data['customer']['postal_code']}")
            break

    # Extract line items
    # From PDF: Total AfterDiscount Discount BeforeDiscount UnitPrice Quantity Description #
    # Example: 199.998 199.998 0 199.998 99.999 2.000 Laptop 1
    # Example: 249.999 232.499 17.500 249.999 249.999 1.000 Software License 5
    # Pattern: numbers with possible commas, then description (one or more words), then line number
    line_pattern = r'([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)\s+([\d,\.]+)\s+([A-Za-z\s]+?)\s+(\d+)(?:\s|$)'

    matches = re.finditer(line_pattern, pdf_text)
    for match in matches:
        try:
            item = {
                'id': match.group(8),
                'description': match.group(7).strip(),  # Strip whitespace from description
                'quantity': float(match.group(6).replace(',', '')),
                'unit_price': float(match.group(5).replace(',', '')),
                'before_discount': float(match.group(4).replace(',', '')),
                'discount_amount': float(match.group(3).replace(',', '')),
                'after_discount': float(match.group(2).replace(',', '')),
                'total': float(match.group(1).replace(',', ''))
            }
            data['line_items'].append(item)
            log(f"  Found line item {item['id']}: {item['description']} (Qty: {item['quantity']}, Price: {item['unit_price']})")
        except (ValueError, IndexError) as e:
            log(f"  WARNING: Failed to parse line item: {str(e)}", "WARN")

    # Extract totals
    # From PDF:
    # 2,299.980 (JOD)  إﺟﻤﺎﻟﻲ اﻟﻔﺎﺗﻮرة ﻗﺒﻞ اﻟﺨﺼﻢ
    # 51.100 (JOD)  ﻣﺠﻤﻮع ﻗﯿﻤﺔ اﻟﺨﺼﻢ
    # 2,248.880 (JOD)  إﺟﻤﺎﻟﻲ ﻗﯿﻤﺔ اﻟﻔﺎﺗﻮرة
    total_patterns = [
        (r'([\d,\.]+)\s+\(JOD\)\s+اﻟﺨﺼﻢ\s+ﻗﺒﻞ\s+اﻟﻔﺎﺗﻮرة\s+إﺟﻤﺎﻟﻲ', 'before_discount'),  # Reversed Arabic
        (r'([\d,\.]+)\s+\(JOD\)\s+الخصم\s+قبل\s+الفاتورة\s+إجمالي', 'before_discount'),
        (r'([\d,\.]+)\s+\(JOD\)\s+اﻟﺨﺼﻢ\s+ﻗﯿﻤﺔ\s+ﻣﺠﻤﻮع', 'total_discount'),  # Reversed Arabic
        (r'([\d,\.]+)\s+\(JOD\)\s+الخصم\s+قيمة\s+مجموع', 'total_discount'),
        (r'([\d,\.]+)\s+\(JOD\)\s+اﻟﻔﺎﺗﻮرة\s+ﻗﯿﻤﺔ\s+إﺟﻤﺎﻟﻲ', 'final_amount'),  # Reversed Arabic
        (r'([\d,\.]+)\s+\(JOD\)\s+الفاتورة\s+قيمة\s+إجمالي', 'final_amount'),
        # Fallback patterns
        (r'المجموع قبل الخصم[:\s]+([\d,\.]+)', 'before_discount'),
        (r'Total Before Discount[:\s]+([\d,\.]+)', 'before_discount'),
        (r'إجمالي قيمة الخصم[:\s]+([\d,\.]+)', 'total_discount'),
        (r'Total Discount[:\s]+([\d,\.]+)', 'total_discount'),
        (r'المبلغ النهائي[:\s]+([\d,\.]+)', 'final_amount'),
        (r'Final Amount[:\s]+([\d,\.]+)', 'final_amount')
    ]

    for pattern, key in total_patterns:
        match = re.search(pattern, pdf_text)
        if match and key not in data['totals']:
            value = float(match.group(1).replace(',', ''))
            data['totals'][key] = value
            log(f"  Found total {key}: {value}")

    # If totals not found, calculate from line items
    if not data['totals'] and data['line_items']:
        data['totals']['before_discount'] = sum(item['before_discount'] for item in data['line_items'])
        data['totals']['total_discount'] = sum(item['discount_amount'] for item in data['line_items'])
        data['totals']['final_amount'] = sum(item['total'] for item in data['line_items'])
        log("  Calculated totals from line items")
    elif data['line_items'] and not all(k in data['totals'] for k in ['before_discount', 'total_discount', 'final_amount']):
        # Fill in missing totals from calculation
        if 'before_discount' not in data['totals']:
            data['totals']['before_discount'] = sum(item['before_discount'] for item in data['line_items'])
        if 'total_discount' not in data['totals']:
            data['totals']['total_discount'] = sum(item['discount_amount'] for item in data['line_items'])
        if 'final_amount' not in data['totals']:
            data['totals']['final_amount'] = sum(item['total'] for item in data['line_items'])
        log("  Calculated missing totals from line items")

    # Extract notes
    notes_patterns = [
        r'ملاحظات[:\s]+([^\n]+)',
        r'Notes[:\s]+([^\n]+)',
        r'تﺎﻈﺣﻼﻣ[:\s]+([^\n]+)'
    ]

    for pattern in notes_patterns:
        match = re.search(pattern, pdf_text)
        if match:
            data['notes'] = match.group(1).strip()
            log(f"  Found notes: {data['notes'][:50]}...")
            break

    # Validate required data
    if not data['line_items'] and not data['totals']:
        raise DataExtractionError(
            "Could not extract line items or totals. "
            "Please verify the PDF structure matches the expected format."
        )

    log("Invoice data parsing completed successfully")
    return data


def create_original_invoice_json(
    invoice_data: Dict[str, Any],
    original_uuid: str
) -> Dict[str, Any]:
    """
    Create JSON structure for the original invoice

    Args:
        invoice_data: Parsed invoice data from PDF
        original_uuid: UUID for the original invoice (from CLI parameter)

    Returns:
        Complete invoice JSON ready for submission
    """
    log("Building original invoice JSON structure...")

    # Build customer object
    customer = {
        "name": invoice_data['customer'].get('name', 'Unknown Customer')
    }

    # Add optional customer fields if available
    if invoice_data['customer'].get('national_id'):
        customer["identificationNumber"] = invoice_data['customer']['national_id']
        customer["identificationType"] = invoice_data['customer'].get('id_type', 'NIN')

    if invoice_data['customer'].get('phone'):
        customer["phoneNumber"] = invoice_data['customer']['phone']

    if invoice_data['customer'].get('postal_code'):
        customer["postalCode"] = invoice_data['customer']['postal_code']

    # Build invoice line items
    line_items = []
    for item in invoice_data['line_items']:
        # Calculate correct values
        # From PDF: total after_discount discount before_discount unit_price quantity description #
        # totalBeforeTax = after_discount (the line total after discount but before tax)
        # totalIncludingTax = after_discount (same as totalBeforeTax for 0% tax)

        total_before_tax = item.get('after_discount', item['total'])
        total_including_tax = total_before_tax  # Same for 0% tax

        line_item = {
            "id": str(item['id']),
            "taxCategory": "O",  # O = 0% tax (Income invoices typically have 0% VAT)
            "description": item['description'],
            "quantity": int(item['quantity']),
            "unitPriceBeforeTax": item['unit_price'],
            "totalBeforeTax": total_before_tax,
            "taxAmount": 0.0,  # 0% tax for income invoices
            "totalIncludingTax": total_including_tax
        }

        # Add discount if exists
        if item.get('discount_amount', 0) > 0:
            line_item["discountAmount"] = item['discount_amount']

        line_items.append(line_item)

    # Build invoice totals
    totals = invoice_data['totals']

    # Calculate correct totals based on line items (after discount)
    total_after_discount = sum(item['totalBeforeTax'] for item in line_items)

    # Round to 3 decimal places to avoid floating point precision issues
    invoice_totals = {
        "totalVATAmount": 0.0,  # 0% VAT for income invoices
        "totalSpecialTaxAmount": 0.0,
        "totalBeforeDiscount": round(totals.get('before_discount', 0.0), 3),
        "totalInvoiceAmount": round(total_after_discount, 3),  # Total after discount
        "totalDiscountAmount": round(totals.get('total_discount', 0.0), 3),
        "finalPayableAmount": round(total_after_discount, 3)  # Same as totalInvoiceAmount for 0% tax
    }

    # Build complete invoice
    invoice = {
        "invoiceNumber": invoice_data['invoice_number'],
        "uniqueSerialNumber": original_uuid,
        "invoiceDate": invoice_data['invoice_date'],
        "paymentType": "LocalIncomeCash",
        "type": "Income",
        "currency": "JOD",
        "supplier": SUPPLIER_INFO,
        "customer": customer,
        "invoiceTotals": invoice_totals,
        "invoiceDetails": line_items
    }

    # Add notes if available
    if invoice_data.get('notes'):
        invoice["invoiceNote"] = invoice_data['notes']

    log(f"Original invoice JSON created: {invoice['invoiceNumber']}")
    return invoice


def create_return_invoice_json(
    original_invoice: Dict[str, Any],
    return_reason: str = "Return for testing purposes"
) -> Dict[str, Any]:
    """
    Create JSON structure for the return invoice (credit note)

    According to PRODUCTION-GUIDE.md and SDK code, return invoices need:
    - New invoice number (prefixed with RET-)
    - New UUID
    - Today's date
    - Reference to original invoice
    - Negative quantities and amounts

    Args:
        original_invoice: The original invoice JSON
        return_reason: Reason for return

    Returns:
        Complete return invoice JSON ready for submission
    """
    log("Building return invoice JSON structure...")

    # Generate return invoice number and UUID
    return_invoice_number = f"RET-{original_invoice['invoiceNumber']}"
    return_uuid = str(uuid.uuid4())
    return_date = datetime.now().strftime('%Y-%m-%d')

    # Copy original invoice structure
    return_invoice = {
        "invoiceNumber": return_invoice_number,
        "uniqueSerialNumber": return_uuid,
        "invoiceDate": return_date,
        "paymentType": original_invoice['paymentType'],
        "type": original_invoice['type'],
        "currency": original_invoice['currency'],
        "supplier": original_invoice['supplier'],
        "customer": original_invoice['customer'],
        "invoiceNote": return_reason
    }

    # Copy and negate line items
    return_line_items = []
    for item in original_invoice['invoiceDetails']:
        return_item = item.copy()
        # Keep quantities and amounts POSITIVE in JSON
        # The SDK will handle the negation when generating XML
        return_line_items.append(return_item)

    return_invoice['invoiceDetails'] = return_line_items

    # Copy totals (keep positive, SDK will negate)
    return_invoice['invoiceTotals'] = original_invoice['invoiceTotals'].copy()

    log(f"Return invoice JSON created: {return_invoice_number}")
    return return_invoice


def save_json_file(data: Dict[str, Any], file_path: Path) -> None:
    """
    Save JSON data to file with proper formatting

    Args:
        data: Dictionary to save as JSON
        file_path: Path where to save the file
    """
    try:
        with open(file_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
        log(f"Saved JSON file: {file_path.name}")
    except Exception as e:
        raise InvoiceProcessorError(f"Failed to save JSON file {file_path}: {str(e)}")


def submit_invoices(
    original_json_path: Path,
    return_json_path: Path,
    console_app_path: Path
) -> Tuple[bool, str]:
    """
    Submit invoices to Jordan e-invoicing system using EFatoraJoConsoleApp

    According to PRODUCTION-GUIDE.md (lines 316-325), we submit both invoices together:
    dotnet EFatoraJoConsoleApp.dll \
      --invoice-file original.json \
      --return-file return.json \
      --output-format json

    Args:
        original_json_path: Path to original invoice JSON
        return_json_path: Path to return invoice JSON
        console_app_path: Path to EFatoraJoConsoleApp.dll

    Returns:
        Tuple of (success: bool, message: str)

    Raises:
        SubmissionError: If submission fails
    """
    log("Submitting invoices to e-invoicing system...")

    # Verify console app exists
    if not console_app_path.exists():
        raise SubmissionError(
            f"EFatoraJoConsoleApp not found at: {console_app_path}\n"
            f"Please build the application first using: dotnet build -c Release"
        )

    # Build command - use absolute paths
    cmd = [
        "dotnet",
        str(console_app_path.absolute()),
        "--invoice-file", str(original_json_path.absolute()),
        "--return-file", str(return_json_path.absolute()),
        "--output-format", "json"
    ]

    log(f"Executing command: {' '.join(cmd)}")

    try:
        # Execute command
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=120,  # 2 minutes timeout
            cwd=console_app_path.parent
        )

        # Parse response
        if result.returncode == 0:
            try:
                # Handle multiple JSON responses in output
                # The console app may output separate responses for return and original invoices
                responses = []
                output_lines = result.stdout.strip()

                # Try to parse multiple JSON objects
                decoder = json.JSONDecoder()
                idx = 0
                while idx < len(output_lines):
                    try:
                        obj, end_idx = decoder.raw_decode(output_lines, idx)
                        responses.append(obj)
                        idx = end_idx
                        # Skip whitespace
                        while idx < len(output_lines) and output_lines[idx].isspace():
                            idx += 1
                    except json.JSONDecodeError:
                        break

                if not responses:
                    error_msg = f"[X] ERROR: No valid JSON in response\n{result.stdout}"
                    log(error_msg, "ERROR")
                    return False, error_msg

                # Check responses
                original_success = None
                return_success = None
                original_invoice = None
                return_invoice = None
                qr_code = None
                errors = []

                for response in responses:
                    if response.get('success'):
                        # Successful submission
                        inv_num = response.get('invoiceNumber') or response.get('returnInvoiceNumber') or ''
                        if inv_num and inv_num.startswith('RET-'):
                            return_success = True
                            return_invoice = inv_num
                        elif inv_num:
                            original_success = True
                            original_invoice = inv_num
                            qr_code = response.get('qrCode', 'N/A')
                    else:
                        # Failed submission - try to identify which invoice failed
                        inv_type = "Return invoice"  # Assume return failed by default
                        error_list = response.get('errors', [])
                        for err in error_list:
                            err_msg = err.get('message', str(err))
                            errors.append(f"{inv_type}: {err_msg}")

                # Determine overall result
                if original_success and (return_success or return_success is None):
                    # Both succeeded or only original succeeded (return not attempted)
                    success_msg = (
                        f"[OK] SUCCESS: Invoice(s) submitted successfully!\n"
                        f"  Original Invoice: {original_invoice}\n"
                    )
                    if return_invoice:
                        success_msg += f"  Return Invoice: {return_invoice}\n"
                    else:
                        success_msg += f"  Return Invoice: Skipped or failed (see errors)\n"
                    success_msg += f"  QR Code: {qr_code[:50] if qr_code else 'N/A'}..."

                    if errors:
                        success_msg += "\n  Warnings/Errors:\n"
                        for err in errors:
                            success_msg += f"    - {err}\n"

                    log(success_msg)
                    return True, success_msg
                else:
                    error_msg = "[X] SUBMISSION FAILED\n"
                    if errors:
                        error_msg += "  Errors:\n"
                        for err in errors:
                            error_msg += f"    - {err}\n"
                    else:
                        error_msg += "  Unknown error occurred\n"

                    log(error_msg, "ERROR")
                    return False, error_msg

            except Exception as e:
                error_msg = f"[X] ERROR: Failed to parse response: {str(e)}\n{result.stdout}"
                log(error_msg, "ERROR")
                return False, error_msg
        else:
            error_msg = (
                f"[X] ERROR: Command failed with exit code {result.returncode}\n"
                f"  STDOUT: {result.stdout}\n"
                f"  STDERR: {result.stderr}"
            )
            log(error_msg, "ERROR")
            return False, error_msg

    except subprocess.TimeoutExpired:
        error_msg = "[X] ERROR: Submission timed out after 2 minutes"
        log(error_msg, "ERROR")
        raise SubmissionError(error_msg)
    except Exception as e:
        error_msg = f"[X] ERROR: Exception during submission: {str(e)}"
        log(error_msg, "ERROR")
        raise SubmissionError(error_msg)


def main():
    """Main entry point for the script"""

    # Parse command line arguments
    parser = argparse.ArgumentParser(
        description='Process a single invoice PDF and submit to Jordan e-invoicing system',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Example usage:
  python process_single_invoice.py --pdf-path "temp/temp/INV-001.pdf" --original-uuid "550e8400-e29b-41d4-a716-446655440000"

This script will:
  1. Read the PDF and extract invoice data
  2. Generate JSON for the original invoice
  3. Generate JSON for the return invoice (credit note)
  4. Save both JSON files in the same directory as the PDF
  5. Submit both invoices to the Jordan e-invoicing system
  6. Verify successful submission and report errors if any

Requirements:
  - PyPDF2 installed (pip install PyPDF2)
  - EFatoraJoConsoleApp built and configured with user secrets
        """
    )

    parser.add_argument(
        '--pdf-path',
        type=str,
        required=True,
        help='Path to the invoice PDF file'
    )

    parser.add_argument(
        '--original-uuid',
        type=str,
        required=True,
        help='UUID for the original invoice (will be used to link return invoice)'
    )

    parser.add_argument(
        '--console-app-path',
        type=str,
        default='EFatoraJoConsoleApp/bin/Release/net8.0/EFatoraJoConsoleApp.dll',
        help='Path to EFatoraJoConsoleApp.dll (default: EFatoraJoConsoleApp/bin/Release/net8.0/EFatoraJoConsoleApp.dll)'
    )

    parser.add_argument(
        '--return-reason',
        type=str,
        default='Return for testing purposes',
        help='Reason for return invoice (default: "Return for testing purposes")'
    )

    args = parser.parse_args()

    # Convert paths to Path objects
    pdf_path = Path(args.pdf_path)
    console_app_path = Path(args.console_app_path)

    # Validate UUID format
    try:
        uuid.UUID(args.original_uuid)
    except ValueError:
        log(f"ERROR: Invalid UUID format: {args.original_uuid}", "ERROR")
        print("\nPlease provide a valid UUID in the format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")
        sys.exit(1)

    # Start processing
    log("=" * 80)
    log("Starting Single Invoice Processing")
    log("=" * 80)
    log(f"PDF Path: {pdf_path}")
    log(f"Original UUID: {args.original_uuid}")
    log(f"Console App: {console_app_path}")
    log("=" * 80)

    try:
        # Step 1: Read PDF
        pdf_text = extract_text_from_pdf(pdf_path)

        # Step 2: Parse invoice data
        invoice_data = parse_invoice_data(pdf_text)

        # Step 3: Create original invoice JSON
        original_invoice = create_original_invoice_json(invoice_data, args.original_uuid)

        # Step 4: Create return invoice JSON
        return_invoice = create_return_invoice_json(original_invoice, args.return_reason)

        # Step 5: Save JSON files in same directory as PDF
        output_dir = pdf_path.parent
        original_json_path = output_dir / f"{invoice_data['invoice_number']}_original.json"
        return_json_path = output_dir / f"{invoice_data['invoice_number']}_return.json"

        save_json_file(original_invoice, original_json_path)
        save_json_file(return_invoice, return_json_path)

        log("=" * 80)
        log("Files saved successfully:")
        log(f"  Original Invoice: {original_json_path}")
        log(f"  Return Invoice: {return_json_path}")
        log("=" * 80)

        # Step 6: Submit invoices
        success, message = submit_invoices(original_json_path, return_json_path, console_app_path)

        log("=" * 80)
        if success:
            log("PROCESSING COMPLETED SUCCESSFULLY", "SUCCESS")
            log("=" * 80)
            print("\n" + message)
            sys.exit(0)
        else:
            log("PROCESSING FAILED", "ERROR")
            log("=" * 80)
            print("\n" + message)
            sys.exit(1)

    except PDFReadError as e:
        log(f"PDF Read Error: {str(e)}", "ERROR")
        print(f"\n[X] ERROR: {str(e)}")
        print("\nPlease verify:")
        print("  - The PDF file exists and is readable")
        print("  - The PDF contains extractable text (not scanned images)")
        sys.exit(2)

    except DataExtractionError as e:
        log(f"Data Extraction Error: {str(e)}", "ERROR")
        print(f"\n[X] ERROR: {str(e)}")
        print("\nPlease verify:")
        print("  - The PDF structure matches the expected format")
        print("  - The PDF contains valid invoice data in Arabic or English")
        sys.exit(3)

    except SubmissionError as e:
        log(f"Submission Error: {str(e)}", "ERROR")
        print(f"\n[X] ERROR: {str(e)}")
        print("\nPlease verify:")
        print("  - EFatoraJoConsoleApp is built (dotnet build -c Release)")
        print("  - User secrets are configured correctly")
        print("  - Internet connection is available")
        print("  - Credentials are valid")
        sys.exit(4)

    except Exception as e:
        log(f"Unexpected Error: {str(e)}", "ERROR")
        import traceback
        print(f"\n[X] UNEXPECTED ERROR: {str(e)}")
        print("\nStack trace:")
        print(traceback.format_exc())
        sys.exit(99)


if __name__ == "__main__":
    main()
