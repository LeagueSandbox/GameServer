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

## An issue should be always created before committing

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

# Tag system

Issues uses tag system that make them easier to identify. Tags are grouped into several groups named with first letter of the group name followed by a short-hand descriptive word or phrase. Tags in group are in same color. Every issue should have relevant tags.

## Area
To-what part of the code is relevant?

| Tag label | Description |
| -------------- | -------------- |
| A-ai | Content releated to Artificial Intelligence |
| A-collision | Content releated to collision |
| A-connection | Content releated to connection |
| A-content-pipeline | Content related to loading |
| A-networking | Content related to networking |
| A-packets	| Content related to packets | 
| A-script-engine | Content related to script engine (items, champion's scripts) |
| A-security | Content related to security |
| A-tools | Content related to tools |
| A-vision | Content related to vision |

## Blocker
Issue is blocking on something before it can be resolved

| Tag label | Description |
| -------------- | ----------- |
| B-reproduce | Blocked on a need to reproduce problem locally |
| B-needs-verification | Blocked due to missing verification |

## Effort
The expected complexity of fixing the issue.

| Tag label | Description |
| -------------- | ----------- |
| E-easy | Can be easily resolved |
| E-good-first-issue | Issue which will help you get into the project |
| E-help-wanted | Assignee/Issue creator requests help? |

## Impact
The effect of the issue remaining unresolved.

| Tag label | Description |
| -------------- | ----------- |
| I-cleanup | No impact; the issue is one of maintainability or tidiness. |
| I-crash | Application crash |
| I-enhancement | No impact; the issue is a missing or proposed feature |
| I-performance | Unnecessary memory usage/Performance degradation |
| I-wrong | An incorrect behaviour is observed (bug) |

## Priority
Priority of the issues.

| Tag label | Description |
| -------------- | ----------- |
| P-high | High priority |
| hacktoberfest | Hacktoberfest, no prefix cause it's a special github label iirc |

## Status
Status of the issues.

| Tag label | Description |
| -------------- | ----------- |
| S-clarifying | This issue needs clarification |
| S-has-open-pr | There is a PR open that resolves the issue |
| S-testing-needed | Needs testing |
| S-unverified | This issue needs verification |
| S-verified | This issue has been verified |
| S-wont-fix | The issue will not be fixed |
| invalid | Used for mark spam hacktoberfest PR's as invalid (they will not count) |
