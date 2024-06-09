---
name: Wiki Update
about: Issue template for updates of the wiki
title: 'Wiki Update'
labels: 'Docs'
assignees: ''
---

Number of commits  
made to the Wiki in the last 24 hours: **{{ env.CommitCount }}**

The wiki was last updated on 
```
Date:           {{ env.UpdatedOn }}
Commit Hash:    [{{ env.Hash }}](https://github.com/axuno/SmartFormat/wiki/_compare/{{ env.Hash }})
Commit Message: "{{ env.CommitMessage }}"
```
