# Synergy project

- [Branch Guidelines](#markdown-header-branch-guidelines)
- [Commit Message Guidelines](#markdown-header-git-commit-guidelines)
- [Push changes to the server](#markdown-header-push-changes-to-the-server)


## Branch Guidelines

Make your changes in a new git branch.
```shell
$ git checkout -b fix/1234_bug-fix
```

### Branch Name Format
Each branch name must be name in special format:

```
<type>/<issue number>_<short_name>
```

### Type
Must be one of the following:

* **feat**: A new feature
* **fix**: A bug fix
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **test**: Adding missing or correcting existing tests
* **chore**: Changes to the build process or some other changes

### Issue number
**Optional** in case there is corresponding issue created

### Short name
Short name should be in Kebab Case

## Git Commit Guidelines

### Commit Message Format
Each commit message must be in special format:

```
<type>(<scope>): <subject>
```

### Type
Must be one of the following:

* **feat**: A new feature
* **fix**: A bug fix
* **refactor**: A code change that neither fixes a bug nor adds a feature
* **test**: Adding missing or correcting existing tests
* **chore**: Changes to the build process or some other changes

### Scope
The scope specifies what part of project has been changed

### Subject
The subject contains succinct description of the change:

* use the imperative, present tense: "change" not "changed" nor "changes"
* capitalize first letter
* no dot (.) at the end

## Push changes to the server

### Git push
Push changes to remote using following `git` command:

```shell
$ git push -u origin fix/1234_bug-fix
```

Once push is done open new pull request and assign it to appropriate people. 