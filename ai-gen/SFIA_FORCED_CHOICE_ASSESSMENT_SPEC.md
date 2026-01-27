# 🧠 SFIA FORCED-CHOICE SKILL LEVEL ASSESSMENT SPEC (AI-READY)

## 0. PURPOSE

This document defines **non-negotiable rules** for generating, evaluating, and confirming
a person's **current SFIA skill level (L1–L7)** using **forced-choice, behavior-based questions**.

The assessment MUST:
- Measure **actual behavioral tendencies**
- Prevent socially desirable or "gaming" responses
- Infer level based on **consistent responsibility patterns**

This spec is designed for **AI-driven test generation and inference**.

---

## 1. CORE PRINCIPLE

> **SFIA level = the highest level of responsibility the person consistently chooses to take in real situations.**

The assessment MUST:
- Avoid testing knowledge or theory
- Avoid moral or "best practice" framing
- Measure what the person is likely to DO under constraints

---

## 2. QUESTION STRUCTURE (MANDATORY)

Each question MUST include ALL components below.

### 2.1 Fixed Context

Each scenario MUST explicitly define:
- Task type (execution / coordination / decision / strategy)
- Risk level (low / medium / high)
- Time pressure (none / moderate / urgent)
- Authority condition (leader available / unavailable)
- Scope clarity (clear / ambiguous)

❌ Forbidden:
- Hypothetical or aspirational phrasing
- "If you want to…"

✅ Required:
- Concrete, realistic operational context

---

### 2.2 Forced-Choice Instruction

The instruction MUST be:

> **Choose the action you are MOST LIKELY to take in real life.**

Forbidden words:
- should
- best
- correct
- ideal

---

### 2.3 Exactly 4 Options

Each question MUST have:
- Exactly 4 options
- Each option representing **one dominant behavior**
- Each option pre-mapped to **one SFIA level only**

No mixed behaviors are allowed.

---

## 3. SFIA LEVEL BEHAVIORAL SIGNATURES (L1–L7)

| Level | Behavioral Core |
|-----|----------------|
| L1 – Follow | Waits for instruction, avoids decisions |
| L2 – Assist | Follows guidance, seeks confirmation |
| L3 – Apply | Acts independently within defined scope |
| L4 – Enable | Owns approach and quality |
| L5 – Ensure / Advise | Influences others, ensures consistency |
| L6 – Initiate / Influence | Initiates change under uncertainty |
| L7 – Set Strategy / Inspire | Sets long-term direction and vision |

Each option MUST map to **exactly one level above**.

---

## 4. TRADE-OFF REQUIREMENT (CRITICAL)

Every question MUST enforce at least one real trade-off:

- Autonomy ↔ Safety
- Speed ↔ Quality
- Responsibility ↔ Escalation
- Initiative ↔ Compliance
- Local optimization ↔ Organizational impact

If no trade-off exists, the question is INVALID.

---

## 5. LEVEL COVERAGE REQUIREMENTS

### 5.1 Minimum Evidence per Level

A level can only be inferred if:
- At least **4 questions** map to that level
- Across **at least 3 distinct contexts**

### 5.2 Context Types

Contexts MUST vary across:
- Low risk vs high risk
- Clear scope vs ambiguous scope
- Individual task vs multi-person impact
- Short-term vs long-term consequences

---

## 6. ANTI-GAMING MECHANISMS

### 6.1 Reverse Questions

At least **25% of questions** MUST:
- Invert context or risk
- Test whether the same behavior holds

Inconsistency lowers confidence.

---

### 6.2 Context Shift Validation

The same behavior MUST be tested under:
- Different authority conditions
- Different risk levels

Behavior collapse under pressure = lower level.

---

## 7. LEVEL CONFIRMATION RULES (NO SCORING)

### 7.1 No Numerical Scores

The AI MUST NOT:
- Assign points
- Calculate averages
- Rank answers numerically

---

### 7.2 Confirmation Criteria for Level L

A level L is **CONFIRMED** if ALL conditions are met:

1. **Consistency**
   ≥ 70% of answers mapped to level L are selected

2. **Context Coverage**
   Answers occur in ≥ 3 distinct context types

3. **Non-Contradiction**
   No dominant fallback to level L-1 in equivalent contexts

---

### 7.3 Final Level Determination
Final Level = Highest SFIA level that satisfies all confirmation criteria
Levels MUST be evaluated top-down:
L7 → L6 → L5 → L4 → L3 → L2 → L1

No averaging between levels is allowed.

---

## 8. LEVEL INVALIDATION RULES

A level MUST be rejected if:

- Behavior appears only in low-risk contexts
- Reverse questions contradict the behavior
- Higher-level behavior disappears under ambiguity or pressure

---

## 9. REPORTING OUTPUT (PER SKILL)

The AI MUST output:

1. Skill name
2. Inferred SFIA level
3. Behavioral evidence (bullet points)
4. Observed limitations
5. Development recommendation

### Forbidden language:
- Lazy
- Weak
- Unmotivated
- Personality-based judgments

Only observable behavior is allowed.

---

## 10. FINAL ASSERTION

If this spec is followed:
- The test measures **real operational behavior**
- SFIA levels are **defensible and auditable**
- Candidates cannot reliably fake higher levels

Violation of this spec invalidates the assessment.
