# Markdown Renderer Feature Showcase

> A comprehensive demo of every supported feature: **GFM**, **KaTeX math**, and **Shiki code highlighting** across all bundled languages.

---

## 1. GitHub Flavored Markdown (GFM)

### Text Formatting

This is **bold**, this is *italic*, this is ~~strikethrough~~, and this is ***bold italic***.

Here's some `inline code` in a sentence.

### Links & Images

- Autolink: https://github.com
- [Named link](https://github.com)

### Blockquote

> "Any fool can write code that a computer can understand.
> Good programmers write code that humans can understand."
>
> — *Martin Fowler*

### Unordered List

- First item
- Second item
  - Nested item A
  - Nested item B
- Third item

### Ordered List

1. Step one
2. Step two
   1. Sub-step A
   2. Sub-step B
3. Step three

### Task List

- [x] Set up unified pipeline
- [x] Add GFM support
- [x] Add KaTeX math rendering
- [x] Add Shiki syntax highlighting
- [x] Add DOMPurify sanitization
- [ ] World domination

### Table

| Language   | Paradigm       | Typing     | First Appeared |
|:-----------|:--------------:|:----------:|---------------:|
| C          | Procedural     | Static     | 1972           |
| Python     | Multi-paradigm | Dynamic    | 1991           |
| TypeScript | Multi-paradigm | Static     | 2012           |
| PHP        | Multi-paradigm | Dynamic    | 1995           |
| C#         | OOP            | Static     | 2000           |
| SQL        | Declarative    | Static     | 1974           |

### Horizontal Rule

---

## 2. KaTeX / LaTeX Math

### Inline Math

The quadratic formula is $x = \frac{-b \pm \sqrt{b^2 - 4ac}}{2a}$ and Euler's identity is $e^{i\pi} + 1 = 0$.

### Block Math

$$
\int_{-\infty}^{\infty} e^{-x^2} \, dx = \sqrt{\pi}
$$

### Matrix

$$
\mathbf{A} = \begin{pmatrix}
1 & 2 & 3 \\
4 & 5 & 6 \\
7 & 8 & 9
\end{pmatrix}
$$

### Summation & Product

$$
\sum_{n=1}^{\infty} \frac{1}{n^2} = \frac{\pi^2}{6}
\qquad
\prod_{i=1}^{n} i = n!
$$

### Aligned Equations

$$
\begin{aligned}
\nabla \cdot \mathbf{E} &= \frac{\rho}{\varepsilon_0} \\
\nabla \cdot \mathbf{B} &= 0 \\
\nabla \times \mathbf{E} &= -\frac{\partial \mathbf{B}}{\partial t} \\
\nabla \times \mathbf{B} &= \mu_0 \mathbf{J} + \mu_0 \varepsilon_0 \frac{\partial \mathbf{E}}{\partial t}
\end{aligned}
$$

---

## 3. Code Blocks (Shiki Highlighted)

### C

```c
#include <stdio.h>

int factorial(int n) {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}

int main(void) {
    for (int i = 0; i <= 10; i++) {
        printf("%2d! = %d\n", i, factorial(i));
    }
    return 0;
}
```

### C++

```cpp
#include <iostream>
#include <vector>
#include <algorithm>

int main() {
    std::vector<int> nums = {5, 3, 8, 1, 9, 2, 7};
    std::sort(nums.begin(), nums.end());

    std::cout << "Sorted: ";
    for (const auto& n : nums) {
        std::cout << n << " ";
    }
    std::cout << std::endl;
    return 0;
}
```

### C\#

```csharp
using System;
using System.Linq;

class Program
{
    static void Main()
    {
        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var evens = numbers.Where(n => n % 2 == 0).Select(n => n * n);

        Console.WriteLine($"Squared evens: {string.Join(", ", evens)}");
    }
}
```

### TypeScript

```typescript
interface User {
  id: number;
  name: string;
  email: string;
}

async function fetchUser(id: number): Promise<User> {
  const res = await fetch(`/api/users/${id}`);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json() as Promise<User>;
}

const user = await fetchUser(42);
console.log(`Hello, ${user.name}!`);
```

### JavaScript

```javascript
function debounce(fn, ms) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => fn(...args), ms);
  };
}

const log = debounce((msg) => console.log(msg), 300);
log("Hello, world!");
```

### Python

```python
from dataclasses import dataclass
from functools import reduce

@dataclass
class Point:
    x: float
    y: float

    def distance_to(self, other: "Point") -> float:
        return ((self.x - other.x) ** 2 + (self.y - other.y) ** 2) ** 0.5

points = [Point(0, 0), Point(3, 4), Point(6, 8)]
total = reduce(lambda acc, pair: acc + pair[0].distance_to(pair[1]),
               zip(points, points[1:]), 0.0)
print(f"Total path length: {total:.2f}")
```

### PHP

```php
<?php

function fibonacci(int $n): array {
    $seq = [0, 1];
    for ($i = 2; $i < $n; $i++) {
        $seq[] = $seq[$i - 1] + $seq[$i - 2];
    }
    return array_slice($seq, 0, $n);
}

$result = fibonacci(12);
echo "Fibonacci: " . implode(", ", $result) . PHP_EOL;
```

### CSS

```css
:root {
  --primary: oklch(0.6 0.2 260);
  --radius: 0.5rem;
}

.card {
  background: var(--primary);
  border-radius: var(--radius);
  padding: 1.5rem;
  box-shadow: 0 4px 6px oklch(0 0 0 / 0.1);
  transition: transform 200ms ease;

  &:hover {
    transform: translateY(-2px);
  }
}
```

### JSON

```json
{
  "name": "markdown-renderer",
  "version": "1.0.0",
  "features": {
    "gfm": true,
    "katex": true,
    "shiki": true,
    "dompurify": true
  },
  "languages": ["c", "cpp", "csharp", "typescript", "javascript", "python", "php", "css", "json", "bash", "sql", "markdown"]
}
```

### Bash

```bash
#!/usr/bin/env bash
set -euo pipefail

readonly PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "🔍 Checking dependencies..."
for cmd in node bun git; do
  if ! command -v "$cmd" &>/dev/null; then
    echo "❌ Missing: $cmd" >&2
    exit 1
  fi
  echo "  ✅ $(command -v "$cmd") → $($cmd --version 2>&1 | head -1)"
done

echo "🚀 All good! Starting build..."
cd "$PROJECT_DIR" && bun run build
```

### SQL

```sql
SELECT
    u.name,
    u.email,
    COUNT(p.id) AS project_count,
    COALESCE(SUM(p.stars), 0) AS total_stars
FROM users u
LEFT JOIN projects p ON p.owner_id = u.id
WHERE u.created_at >= '2025-01-01'
GROUP BY u.id, u.name, u.email
HAVING COUNT(p.id) > 0
ORDER BY total_stars DESC
LIMIT 10;
```

### Markdown

```markdown
# Hello World

This is a **markdown** code block _inside_ markdown.

- Bullet point
- [A link](https://example.com)

> A blockquote inside a code block.
```

---

## 4. Edge Cases

### Empty Code Block

```
plain text with no language specified
```

### Deeply Nested Lists

1. Level 1
   - Level 2
     - Level 3
       - Level 4

### Long Inline Math

The probability density function of the normal distribution is $f(x) = \frac{1}{\sigma\sqrt{2\pi}} e^{-\frac{1}{2}\left(\frac{x-\mu}{\sigma}\right)^2}$ which is quite elegant.

### XSS Test (should be sanitized)

<script>alert('xss')</script>

<img src=x onerror="alert('xss')">

<a href="javascript:alert('xss')">click me</a>

---

*Rendered with unified + remark-gfm + remark-math + rehype-katex + shiki + DOMPurify* ✨
