# Line spacing

This article is about the definition of line spacing in MigraDoc.

## Line positioning

The line positioning, depending on the line spacing, should be identical to the positioning done by Microsoft Word.  

The baseline is calculated as follows.

### Relative line spacings

For a line’s baseline position in a paragraph using the **Single**, **OnePtFive**, **Double** or **Multiple** line spacing,
the **maximum** of the **paragraph’s baseline** and all **FormattedText’s baselines** is used.

All baselines used for calculation are the result of the used **font size multiplied with the given line spacing value**.

### Absolute line spacings

#### LineSpacingRule.AtLeast

For a line’s baseline position in a paragraph using the **AtLeast** line spacing,
the **maximum** of the **paragraph’s baseline** and all **FormattedText’s baselines** is used.

All baselines used for calculation are the **maximum** of the **used font size** and the **paragraph’s line spacing value**.

#### LineSpacingRule.Exactly

For a line’s baseline position in a paragraph using the **Exactly** line spacing,
**only the paragraph’s font and line spacing** are considered.
This way, FormattedTexts with a larger font size have no impact on the line’s baseline position.

The baseline is calculated by using the **paragraph’s font baseline scaled to the paragraph’s line spacing**.
