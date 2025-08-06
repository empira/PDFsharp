# String parsing

Parsing strings has grown over the years and needs to be reviewed.
Collect issues about strings and names here.

## Names

* Spec says a name can contain any character, but parsers generally use e.g. backslash as a delimiter.

## Strings in PDF objects

Check the following:

* Octal characters. Are illegal octal numbers (8 and 9) handled correctly?
* Escape characters. PDF specs states that the reverse solidus must be ignored.
* Nesting of '(' and ')'. Is it always handled correctly?
  Should we optimize Writer if parenthesis occurrences are leveled equally? Currently we always escape parenthesis.
* Hex strings <…>
  * Is encryption always identified correctly?
  * Can it be encoded big endian? Check how Acrobat handles such strings.
  * How to decided if it is 8 or 16 bit. Digest is 8 bit, Unicode is 16 bit.
  * Can it contain glyph indices outside a content stream?

## Strings in content streams

* `(…)` string. Is this ASCII only?
* `<…>` string. Is this Glyph ID only?
