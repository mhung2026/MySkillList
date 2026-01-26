# 🧠 FORCED-CHOICE QUESTION GENERATION SPEC (SFIA-BASED)

## 0. PURPOSE OF THIS SPEC

This document defines **strict rules** for generating forced-choice (SJT-style) questions
to assess **SFIA skill levels based on real behavior**, not theoretical knowledge.

The goal is to:
- Expose **actual behavioral tendencies**
- Prevent candidates from gaming the test
- Enable **reliable skill-level inference**

This spec is intended for **AI-based question generation**.

---

## 1. CORE PRINCIPLE

> **Measure what the person is likely to DO under responsibility, not what they know is correct.**

The test MUST:
- Avoid moral language
- Avoid “best practice” framing
- Avoid knowledge recall

---

## 2. STRUCTURE OF A VALID QUESTION

Each question MUST contain **ALL** of the following:

### 2.1 Fixed Context
The scenario must explicitly define:
- Task type (bug fix, feature, analysis, decision, coordination)
- Constraints (deadline, documentation, risk level)
- Authority conditions (leader available or not)
- Scope clarity (clear / ambiguous)

❌ Invalid:
> If you want to…

✅ Valid:
> The deadline is today, documentation exists, the leader is unavailable.

---

### 2.2 Forced-Choice Instruction

The instruction MUST say:
> **Choose the action you are MOST LIKELY to take in real life**

Forbidden words:
- should
- best
- correct
- ideal

---

### 2.3 Exactly 4 Options (No more, no less)

Each option must:
- Be realistic
- Represent a **distinct behavioral strategy**
- Have a **clear SFIA level mapping**

---

## 3. OPTION DESIGN RULES (CRITICAL)

### 3.1 No “Correct” Answer
There MUST NOT be:
- Obviously good answers
- Obviously bad answers

Every option must be defensible.

---

### 3.2 One Option = One Dominant Behavior

Each option MUST represent **one primary behavior dimension**:

| Behavior Dimension | Example |
|-------------------|--------|
| Dependency | Waiting for approval |
| Autonomy | Acting within scope |
| Risk avoidance | Minimizing exposure |
| Speed bias | Acting before validation |
| Peer reliance | Seeking consensus |

Mixed behaviors are NOT allowed.

---

### 3.3 Level Mapping Template

Each option MUST map to a **single SFIA level**:

| SFIA Level | Behavioral Signature |
|-----------|---------------------|
| L1 | Requires close supervision |
| L2 | Follows instructions, seeks confirmation |
| L3 | Works independently within defined scope |
| L4 | Takes responsibility for approach and quality |

Options MUST NOT jump across levels.

---

## 4. TRADE-OFF ENFORCEMENT

Every question MUST force a trade-off between at least TWO of the following:

- Autonomy ↔ Safety
- Speed ↔ Quality
- Responsibility ↔ Escalation
- Initiative ↔ Compliance

If no trade-off exists, the question is INVALID.

---

## 5. LEVEL COVERAGE REQUIREMENT

### 5.1 Minimum Questions per Level

Each assessed level MUST have:
- At least **4 questions**
- Across **different contexts**

Example for Level 3:
- Bug fix
- New task
- Ambiguous requirement
- Deadline pressure

---

### 5.2 No Single-Question Decisions

Level inference MUST NEVER rely on:
- One question
- One scenario

Patterns are required.

---

## 6. CONSISTENCY & ANTI-GAMING MECHANISMS

### 6.1 Reverse Questions
At least **25% of questions** must be reversals.

Example:
- Q1: What are you most likely to do?
- Q6: When would you NOT do that?

---

### 6.2 Context Shift
The same behavior must be tested under:
- Low risk
- High risk
- Clear scope
- Ambiguous scope

Inconsistency = lower confidence level.

---

## 7. SCORING RULES (AI MUST FOLLOW)

### 7.1 No Numerical Scores

DO NOT:
- Assign total points
- Average scores

---

### 7.2 Highest Consistent Behaviour Rule

- Group answers by SFIA level
- A level is considered **achieved** if:
  - ≥ 70% of answers mapped to that level are selected
  - Across at least 3 distinct contexts

The **highest level meeting this rule** is the final result.

---

## 8. REPORTING REQUIREMENTS

### 8.1 Report Structure (Per Skill)

Each report MUST include:

1. Skill name
2. Inferred SFIA level
3. Behavioral evidence (bullet points)
4. Observed limitations
5. Development recommendation

---

### 8.2 Forbidden Report Language

DO NOT use:
- Lazy
- Unmotivated
- Bad attitude
- Weak personality

Use only **observable behavior descriptions**.

---

## 9. SAMPLE QUESTION (REFERENCE ONLY)

> A minor bug is reported. Documentation exists.  
> The deadline is today. The leader is unavailable.

**Choose the action you are MOST LIKELY to take in real life:**

- A. Wait for leader confirmation before fixing  
  *(L2 – dependency)*

- B. Fix it following the guideline and report afterward  
  *(L3 – autonomous within scope)*

- C. Quickly fix the obvious part and test later  
  *(L2– speed bias)*

- D. Ask a colleague to review or handle it together  
  *(L2.5 – peer reliance)*

---

## 10. AI GENERATION CONSTRAINTS (NON-NEGOTIABLE)

AI MUST:
- Generate behavior-driven scenarios
- Maintain realism
- Enforce trade-offs
- Hide SFIA levels from candidates

AI MUST NOT:
- Ask knowledge questions
- Reveal “best practice”
- Reward socially desirable answers

---

## 11. FINAL ASSERTION

If this spec is followed:
- Candidates cannot reliably fake higher levels
- Results reflect **operational behavior**
- SFIA levels become defensible and auditable

If this spec is violated:
- The test degenerates into knowledge assessment
- Results lose predictive validity

