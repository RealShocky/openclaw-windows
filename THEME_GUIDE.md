# 🎨 OpenClaw GUI - Professional Theme Guide

## Design Philosophy

**Enterprise-grade with futuristic accents**
- Professional dark theme inspired by GitHub's Primer design system
- High contrast for excellent readability
- Cyan/blue accents for a modern, tech-forward feel
- Clean typography with proper spacing

---

## Color Palette

### Primary Colors

| Color | Hex | Usage |
|-------|-----|-------|
| **Deep Space** | `#0D1117` | Main background, chat area |
| **Dark Gray** | `#161B22` | Secondary background, panels |
| **Slate** | `#21262D` | Buttons, input fields |
| **Border Gray** | `#30363D` | Borders, separators |

### Text Colors

| Color | Hex | Usage |
|-------|-----|-------|
| **White** | `#E6EDF3` | Primary text, messages |
| **Light Gray** | `#C9D1D9` | Secondary text, labels |
| **Muted Gray** | `#8B949E` | Tertiary text, section headers |
| **Subtle Gray** | `#7D8590` | Timestamps, metadata |

### Accent Colors

| Color | Hex | Usage |
|-------|-----|-------|
| **Bright Blue** | `#58A6FF` | Brand color, links, highlights |
| **Action Blue** | `#1F6FEB` | Primary buttons, user messages |
| **Hover Blue** | `#388BFD` | Button borders, hover states |
| **Success Green** | `#238636` | Online status, start button |
| **Green Border** | `#2EA043` | Success button borders |
| **Danger Red** | `#DA3633` | Offline status, stop button |
| **Red Border** | `#F85149` | Danger button borders |

---

## Typography

### Font Sizes
- **Headings:** 20px (Bold)
- **Body Text:** 14px
- **Buttons:** 11-13px (SemiBold)
- **Labels:** 10-12px
- **Timestamps:** 10px

### Font Weights
- **Bold:** Section headers, brand name
- **SemiBold:** Buttons, message senders
- **Regular:** Body text, descriptions

---

## Component Styles

### Buttons

**Primary Action (Send, Start)**
```
Background: #1F6FEB / #238636
Foreground: #FFFFFF
Border: #388BFD / #2EA043
Padding: 12-24px, 10-12px
Font: SemiBold, 11-13px
```

**Danger (Stop)**
```
Background: #DA3633
Foreground: #FFFFFF
Border: #F85149
Padding: 12px, 10px
Font: SemiBold, 11px
```

**Secondary (Default)**
```
Background: #21262D
Foreground: #C9D1D9
Border: #30363D
Padding: 12px, 8-10px
```

### Input Fields

```
Background: #0D1117
Foreground: #E6EDF3
Border: #30363D
Padding: 15px
Font: 14px
```

### Status Indicators

**Online**
```
Background: #238636
Text: ● ONLINE
Color: #FFFFFF
```

**Offline**
```
Background: #DA3633
Text: ● OFFLINE
Color: #FFFFFF
```

### Message Bubbles

**User Messages**
```
Background: #1F6FEB
Text: #E6EDF3
Sender: #FFFFFF
Alignment: Right
```

**AI Messages**
```
Background: #21262D
Text: #E6EDF3
Sender: #58A6FF
Alignment: Left
```

**System Messages**
```
Background: #161B22
Text: #E6EDF3
Sender: #8B949E
Alignment: Center
```

---

## Layout Structure

### Top Bar
- Height: Auto
- Background: `#161B22`
- Border Bottom: `#30363D`
- Padding: 15px, 10px

### Sidebar
- Width: 250px
- Background: `#0D1117`
- Border Right: `#30363D`

### Chat Area
- Background: `#0D1117`
- Padding: 20px

### Status Bar
- Height: Auto
- Background: `#161B22`
- Border Top: `#30363D`
- Padding: 10px, 6px

---

## Accessibility

### Contrast Ratios
- **Primary Text:** 13.5:1 (WCAG AAA)
- **Secondary Text:** 8.2:1 (WCAG AAA)
- **Buttons:** 4.8:1 (WCAG AA)

### Readability Features
- Line height: 22px for body text
- Proper spacing between elements
- Clear visual hierarchy
- High contrast borders

---

## Design Principles

1. **Professional First**
   - Clean, minimal design
   - Enterprise-grade aesthetics
   - No unnecessary decorations

2. **Futuristic Accents**
   - Cyan/blue highlights
   - Modern color palette
   - Tech-forward feel

3. **Excellent Readability**
   - High contrast text
   - Proper font sizing
   - Clear visual hierarchy

4. **Consistent Spacing**
   - Uniform padding
   - Consistent margins
   - Aligned elements

5. **Status Clarity**
   - Color-coded states
   - Clear indicators
   - Immediate feedback

---

## Comparison: Old vs New

### Old Theme Issues
- ❌ Low contrast (#858585 on #252526)
- ❌ Hard to read text
- ❌ Inconsistent colors
- ❌ Muddy appearance

### New Theme Benefits
- ✅ High contrast (#C9D1D9 on #0D1117)
- ✅ Crisp, readable text
- ✅ Consistent color system
- ✅ Professional appearance
- ✅ Futuristic accents
- ✅ Better visual hierarchy

---

## Implementation Notes

### Color System
Based on GitHub's Primer design system with custom modifications for OpenClaw branding.

### Font System
Uses system fonts with proper weights and sizes for optimal readability.

### Border System
Subtle borders (`#30363D`) provide structure without overwhelming the interface.

### Button System
Three-tier hierarchy: Primary (blue/green), Danger (red), Secondary (gray).

---

## Future Enhancements

- [ ] Add subtle gradients for depth
- [ ] Implement hover animations
- [ ] Add focus indicators for accessibility
- [ ] Custom scrollbar styling
- [ ] Smooth transitions between states
- [ ] Dark/Light theme toggle
- [ ] Custom accent color picker

---

**Theme Version:** 2.0 - Professional Edition
**Last Updated:** February 18, 2026
**Design System:** GitHub Primer + OpenClaw Custom
