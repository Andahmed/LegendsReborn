# Contributing

## Roles (12 people)
PM/Producer (1) - Systems Designer (1) - Economy Designer (1) - Client: Combat (1) - Client: UI (1) - Client: Systems (1) - Server (2) - Concept/UI Artist (1) - 3D Artist (1) - Animation/VFX (1) - QA (1).
Every source file starts with a header: OWNER ROLE / RESPONSIBILITY / WORK ON / STATUS. Keep it updated when you change the file.

## Rhythm
2-week sprints, 15-min daily stand-up, Review + Planning at sprint end. Weekly status report from the PM every Thursday.

## Definition of Done
Written + tested + code-reviewed + works on a real device + designer-approved (art/design) + QA-signed.

## Git
- `main` always builds. Work on `feature/<name>`, open a PR, one reviewer.
- Commit messages: imperative, short ("Add energy regen").
- Large art goes through Git LFS; never put unused art packs in `Resources/`.
- Do not commit Library/, Temp/, Builds/.

## Work log
Append an entry to `docs/WORKLOG.md` at the end of every working session: date, who, what changed (files), what is unverified, what is next.
