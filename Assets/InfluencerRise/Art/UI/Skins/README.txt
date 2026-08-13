Skin slot convention (Influencer Rise UI)
==========================================

Every background panel in the UI (row backgrounds, popup background, top bar,
bottom tab bar, screen background) uses a uGUI Image component configured as:

    Image Type = Sliced
    Sprite     = one of the placeholder sprites in this folder

This is deliberate architecture, not final art. Swapping in real texture art
later means replacing ONLY the sprite reference on these Image components -
no layout, script, or prefab structure changes required, since Sliced/9-slice
scaling already handles arbitrary panel sizes correctly regardless of the
underlying art's resolution or border thickness.

Files:
- Skin_PanelPlaceholder.png    - generic rectangular panel skin (9-slice,
  6px border). Used by: row backgrounds, popup background, top bar
  background, bottom tab bar background, screen background (via tint/color
  variation on the SAME sprite to differentiate "raised panel" from
  "background" without needing separate art yet).
- Skin_IconFramePlaceholder.png - circular icon-frame skin (Simple, not
  sliced - a true 9-slice does not apply meaningfully to a circular shape).
  Used by: row icon frames, popup's larger art placeholder slot.

When real art arrives: replace the Sprite field on each Image component.
Do not restructure the hierarchy or re-author layout to do this swap.
