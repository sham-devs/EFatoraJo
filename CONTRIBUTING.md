# Contributing

We ❤️ contributions and welcome your pull requests!

## Code of Conduct

Please review our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## Getting Started

Follow the standard GitHub workflow to contribute to **JoFatora .NET Client**:

1. **Fork** the repository to your own GitHub account.

2. **Clone** your fork locally:

```bash
git clone https://github.com/<your-username>/jofatora-dotnet-sdk.git
cd jofatora-dotnet-sdk
```

3. **Configure the upstream remote** (to keep your fork updated):

```bash
git remote add upstream https://github.com/ShamDevs/jofatora-dotnet-sdk.git
```

4. **Keep your fork synchronized** with upstream:

```bash
git fetch upstream
git checkout main
git merge upstream/main
git push origin main
```

5. **Create a new branch** for your changes:

```bash
git checkout -b feature/your-feature-name
# or for bug fixes:
git checkout -b fix/issue-description
```

## Development Guidelines

### Before Making Changes

- Review our [coding standards](CODING_STANDARDS.md)
- Check existing [issues](https://github.com/ShamDevs/jofatora-dotnet-sdk/issues) and [pull requests](https://github.com/ShamDevs/jofatora-dotnet-sdk/pulls)
- For major changes, consider opening an issue first to discuss

### Making Changes

6. **Make your changes**, ensuring that:
   - Code follows the project's style and conventions
   - New functionality is covered by tests
   - XML documentation is added for public APIs
   - Breaking changes are documented

7. **Format your code** before committing:

```bash
dotnet format
```

8. **Run all tests** and ensure they pass:

```bash
dotnet test
dotnet test --configuration Release
```

9. **Run static analysis** (if applicable):

```bash
dotnet build --verbosity normal
```

### Committing Changes

10. **Commit** your changes with a clear, descriptive message following [Conventional Commits](https://www.conventionalcommits.org/):

```bash
# Examples:
git commit -m "feat: add support for async batch operations"
git commit -m "fix: resolve null reference in authentication flow"
git commit -m "docs: update API documentation for v2.0"
git commit -m "test: add unit tests for payment validation"
```

11. **Keep your branch up to date** with upstream before pushing:

```bash
git fetch upstream
git rebase upstream/main
```

12. **Push** your branch to your fork:

```bash
git push origin feature/your-feature-name
```

## Pull Request Process

13. **Open a Pull Request** from your fork's branch to the `main` branch of the original repository.

14. **Fill out the PR template** with:
    - Clear description of changes
    - Link to related issues (e.g., "Closes #123")
    - Screenshots/examples if applicable
    - Checklist confirmation

15. **Respond to review feedback** promptly and make necessary updates until your PR is approved and merged.

## Types of Contributions

We welcome various types of contributions:

- 🐛 **Bug fixes**
- ✨ **New features**
- 📚 **Documentation improvements**
- 🧪 **Test coverage enhancements**
- 🎨 **Code quality improvements**
- 🔧 **Build/CI improvements**

## Coding Standards

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and under 50 lines when possible
- Write unit tests for new functionality (aim for >80% coverage)

## Commit Message Guidelines

We use [Conventional Commits](https://www.conventionalcommits.org/) format:

- `feat:` new features
- `fix:` bug fixes
- `docs:` documentation changes
- `test:` adding or updating tests
- `refactor:` code refactoring
- `perf:` performance improvements
- `chore:` maintenance tasks

## Questions?

- 💬 Join our [Discussions](https://github.com/ShamDevs/jofatora-dotnet-sdk/discussions)
- 🐛 Report bugs via [Issues](https://github.com/ShamDevs/jofatora-dotnet-sdk/issues)
- 📧 Email us at [contributors@jofatora.com](mailto:contributors@jofatora.com)

## Recognition

Contributors will be acknowledged in our [CONTRIBUTORS.md](CONTRIBUTORS.md) file and release notes.

Thank you for contributing to JoFatora .NET Client! 🚀
