# Contributing
We're looking for people interested in contributing to the project.  
Currently the technologies we use include:
* C#
* Python
* Electron
* Node.js
* Angular
* Socket.io

For more detailed project specifications head over to https://leaguesandbox.github.io/  
If you're interested in contributing, come find us from [Discord](https://discord.gg/0vmmZ6VAwXB05gB6) and let us know.

# Commits

## Always create an issue before committing

All commits should refer to issues. This is simply so that we can better track what is being worked on or what should be worked on.

## Commit Structure:
```
Issue Reference -- Title of the commit

Description of the commit

Issue reference
```
For example:
```
Resolve #123 -- Implement a foobar

Implement foo to the bar module

Refs #123
```

You can also use `Progress` in place of `Resolve`, in case the issue was not resolved yet.

**THIS GUIDE CONCERNS _COMMIT_ MESSAGES, AND NOT _PULL REQUEST_ MESSAGES**
# Project policies
* Line length should be 120 characters maximum whenever possible (use Editor Guidelines plugin for a ruler)
* Pull requests must be approved before they can be merged
* Pull requests should not be merged before the build has passed
    * If the build fails, ping the pull request creator and tell him to fix it
* Files and folders in `PascalCase`
* JSON dictionary keys in `PascalCase`
* Keep the code as simple and clear to read as possible
* Each separate feature should be developed in their own branch
* Commits should be in logical small pieces
* Pull requests should be kept as small as possible, generally one feature per pull requests
    * Instead of submitting one huge pull request with 3 features, submit each feature individually

# C# guidelines
* Function names in `PascalCase`
* Constants in `ALL_CAPS`
* Private variables in `_camelCaseWithUnderscore`
* Public properties as getters / setters in `PascalCase`
* All `public` variable access should happen through getters / setters
* `#region`s shouldn't be used, instead split code into classes/files when needed
* Dictionaries preferred over `switch`es and long `if/else` statements
* Boolean variable names should be prefixed with a question (is/can/should)
* All `if/else/while/for/foreach/try/catch/finally/lock` clauses should be wrapped inside brackets (`{}`), even if they are only one line.
* Conditional operator should be avoided. `condition ? option1 : option2`
    * This is fine to use in some niche cases where you can't avoid using it
* Interpolated strings with embedded logic should not be used

# Development flow and how to use git shell
**Using git shell is strongly encouraged**

1. Pull latest version of indev
    * `git fetch -p`
    * `git pull origin indev`
2. Checkout to a new branch
    * `git checkout -b <branch_name>`
3. Make changes, do commits
    * `git status` - List of changed files
    * `git add <filename>` - Stage file for commit
    * `git add -u` - Stage all updated files for commit
    * `git add -A` - Stage all unstaged files for commit
    * `git commit -m "<commit message>"` - Create commit
4. Push to github
    * `git push origin <branch_name>`
5. Create pull request
6. Checkout back to indev
    * `git checkout indev`
7. Repeat
