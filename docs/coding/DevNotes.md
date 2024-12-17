# Developer notes

UNDER CONSTRUCTION

Just here before being moved to an appropriate place.

## Temporary tags

Temporary tags are allowed in develop branch, but must be removed in releases.

Create a unit test that fails if a temporary tag is in the C# code.

### TODO

**Here is something to be done __before the project is released__.**

### BUG

**Here is definitely wrong code that must be fixed before the project is released.**

### HACK

This is a quick fix done during code review, development, finding a bug.

### REVIEW

This code must be discussed with another developer who has more knowledge about the issue.

## General hash tags

### CHECK_BEFORE_RELEASE

Check this code here before you finalize the code for a release branch.

### DELETE yyyy-mm-dd

Here is code that was replaced by newer code and should be deleted in the future.
But kept here at the moment as reference in case the new code has bugs.

After the specified date the code should be deleted.

### KEEP

Here is older code that is not used anymore but kept here for 
documentation or reference purposes and shall not be removed.

* Type `KEEP`, not `#KEEP`.

**Example**

```cs
// KEEP for future reference.
// …
```

### IMPROVE

Here is code that substantially works but has potential for improvements
for better reliability.

**Example**

```cs
// IMPROVE robustness of the code.
// …
```

### OBSERVATION

### EXPERIMENTAL

### TEST

Here is code that should be covered by (more) unit tests.

## Clean up old code

Remove all this stuff from existing code.

* Hack:
* Note:
* Magic:
* ToDo
* UNDONE
* TODOWPF
* ReviewSTLA
* THHO4STLA
* … (there are other randomly used tags)

## `#if true` condition

We often use the following technique to toggle old and new implementations.

```CS
#if true
    «new code»
#else
    «old code»
#endif
```

>> Define how to treat this code.

## Contextual hash tags

A contextual hash tag marks code that was written in connection with a particular feature.

| Hash tag          | Description                                |
| :---------------- | :----------------------------------------- |
| #PageOrientation  | Page size and orientation                  |
| #PDF-A            | Changes for PDF/A                          |
| #PDF-UA           | Code belonging to PDF/UA                   |
|                   |                                            |
|                   |                                            |
