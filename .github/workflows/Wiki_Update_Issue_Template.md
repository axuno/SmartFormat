---
name: Wiki Update
about: Issue template for updates of the wiki
title: 'Wiki Update'
labels: 'Docs'
assignees: ''
---

Number of commits  
made to the Wiki in the last 24 hours: **{{ env.CommitCount }}**

These are the details of the latest commit: 

| Item  | Value  |
|:---|:---|
| Date | {{ env.UpdatedOn }} |
| Hash | [{{ env.Hash }}](https://github.com/axuno/SmartFormat/wiki/_compare/{{ env.Hash }}) |
| Message | {{ env.CommitMessage }} |
