# Admin Web Design System

## Direction

The admin web is an operational workspace, not a marketing site. The UI should feel compact, calm, and easy to scan for repeated management tasks.

- Style: shadcn-like, clean, restrained.
- Layout: full-screen admin shell.
- Density: compact by default.
- Color: mostly black, white, neutral gray, with dark blue as the main accent.
- Icons: use `@phosphor-icons/react` only.

Avoid oversized hero sections, decorative gradients, floating page containers, and colorful palettes that do not support the workflow.

## Design Tokens

Design tokens live in:

```text
web/admin/src/styles/globals.css
```

Use the CSS variables instead of hardcoded colors whenever possible.

Core light tokens:

```css
--radius: 0.625rem;
--background: oklch(1 0 0);
--foreground: oklch(0.145 0 0);
--card: oklch(1 0 0);
--primary: oklch(0.214 0.065 254);
--primary-foreground: oklch(0.985 0 0);
--secondary: oklch(0.965 0.005 195);
--muted: oklch(0.965 0.004 195);
--muted-foreground: oklch(0.500 0.020 195);
--accent: oklch(0.955 0.020 142);
--destructive: oklch(0.577 0.245 27.325);
--border: oklch(0.922 0.004 195);
--ring: oklch(0.214 0.065 254);
```

Primary dark blue is used for:

- selected navigation items,
- primary actions,
- focus rings,
- key highlights.

## Typography

Keep typography compact.

- Page title: `text-2xl` to `text-3xl`, `font-semibold`.
- Section/card title: `text-sm` to `text-base`, `font-semibold`.
- Navigation text: `text-[13px]`, `font-semibold`.
- Table/body text: `text-[13px]` or `text-sm`.
- Metadata/helper text: `text-xs` or `text-[13px] text-muted-foreground`.
- Uppercase labels may use tracking, for example `tracking-[0.18em]`.

Do not use large display text inside management screens.

## Spacing And Radius

Compact defaults:

- Page padding: `px-5 py-6`, `lg:px-9`.
- Sidebar padding: `px-4 py-5`.
- Card padding: `p-4`.
- Grid gap: `gap-4`.
- Header bottom padding: `pb-5`.
- Nav item height: `h-9`.
- Sub-nav item height: `h-8`.
- Button height: `h-8` or `h-9`.
- Normal radius: `rounded-lg`.
- Card radius: `rounded-xl`.

Avoid nested large rounded containers for the main dashboard shell.

## Main Shell

Use a full-screen admin shell:

- Sidebar fixed on the left inside the page flow.
- Sidebar stays at viewport height while the main content scrolls independently.
- The primary navigation owns its own vertical scroll when it cannot fit; utility actions and account identity remain visible at the bottom.
- Main content fills the remaining width.
- No floating wrapper around sidebar and content.
- Sidebar has a right border.
- Content area owns page header, metrics, tables, filters, and forms.

The shell should look like one workspace, not separated blocks.

## Sidebar Navigation

Sidebar behavior:

- Logo and product name stay at the top-left.
- Top-level item selection uses dark blue background and white text.
- A category item can expand/collapse child options.
- Clicking a category should select its first child option.
- Expanded category itself does not need a selected background.
- Child option selection should use the same dark blue selected treatment as top-level items.
- Long option labels must truncate instead of wrapping.
- Use connector lines between expanded category and child options.
- Related account screens live under `Accounts`, with `Admins` and `Customers` as children.
- Logout stays at the bottom of the sidebar.
- Logout uses destructive red styling and centered text; no icon needed.

## Cards And Data Blocks

Cards are for dashboard data, forms, tables, and summaries.

- Use subtle border.
- Avoid heavy shadow.
- Keep radius small: `rounded-xl`.
- Keep padding compact: usually `p-4`.
- Do not put the whole page inside a giant card.
- Metrics should be easy to scan: label, value, small trend badge.
- Related identity and permission details share one card with clearly separated vertical sections.
- Role detail uses the complete read-only permission matrix and marks only permissions assigned to that role.

## Tables

Tables should be dense and readable.

- Header: uppercase, `text-[11px]`, muted.
- Body: `text-[13px]`.
- Cell padding: `px-3 py-3`.
- Use border separators.
- Prefer table-first layouts for growing records.
- Growing lists reuse `ListPagination` instead of implementing page controls per feature.
- Pagination uses icon-only previous/next controls and a numeric rows-per-page input limited to `1–100`.
- Read-only permission catalogs use a matrix: groups as rows, actions as columns, and compact checked/empty boxes as cells.
- Selectable permission matrices provide both an all-permissions control and one bulk-selection control per permission group.

## Page Headers

- Eyebrow, page title, description, and actions use separate vertical rows.
- The page title must own its full row.
- Primary page actions such as `Create` appear below the title/description, aligned to the left.
- Use the shared `PageHeader` component instead of arranging actions per page.
- Nested create, detail, and edit pages use compact breadcrumbs instead of a large back button.

## Buttons

Use shared button primitives.

- Primary: dark blue.
- Secondary: outline or muted background.
- Destructive: red.
- Icon buttons only when the icon clearly communicates the action.
- Text + icon buttons should keep icon size consistent.
- Disabled/loading state must be visible.

## Badges

Badge usage:

- `success`: active/enabled/positive state.
- `warning`: pending/risky non-error state.
- `secondary`: neutral metadata.
- `destructive`: error/destructive state if needed.

Badges should remain small and not dominate tables or cards.

## Forms

Forms should be direct and compact.

- Create and edit workflows use dedicated full pages inside the admin shell.
- Reserve dialogs for short confirmations, especially destructive actions.
- Labels are short.
- Errors appear under the related field.
- Form-level errors appear above the submit action or above the form.
- Submit button shows disabled/loading state during requests.
- Do not rely only on color for errors.

## Empty And Error States

- Empty states should explain what is missing and provide one clear next action.
- Not found page should provide return/logout actions.
- Error messages should be concise and actionable.

## Internationalization UX

- Language switcher stays fixed at the top-right corner.
- Do not add extra icons to the language switcher.
- Real user-facing product text should be localizable.
- Temporary placeholder/sample dashboard data can stay hardcoded until the real feature is implemented.
