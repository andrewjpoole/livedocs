# livedocs

A live rendered blend of markdown documentation, with mermaid diagrams, injected with live data from APIs and SQL queries

## Why?

When documentation is split between wikis, repos, diagrams and up-to-date state info lives in yet more disparate places, it can be hard to figure out whats going on vs what should be going on etc. 
This is a PoC for an idea which offers a solution to this problem by bringing markdown documentation and diagrams together and injecting live state data from various sources.

## What?

a markdown file containing mermaid diagrams are maintained in wikis/repos/file server etc, markdown contains replacement tokens: `{{TestDb.sp_get_current_processing_state}}`
a corresponding json file contains replacement config:
```json
    {
      "Match": "swift-bacs-inbound-message",
      "Instruction": "SvcBusMessageInfo"
    },
```
a backend process periodically reads the markdown and the json and uses the config to fetch the data and replaces the tokens with the data
ideally the backend will push changes to any connected frontend clients, who will see the live state data updating in near real time :)

## TODO

* [x] serve Blazor Wasm app from server
* [x] add Azure AD auth to frontend (anyone from organisation)
* [x] add Azure AD auth to server (service principal with appropriate permissions)
* [x] add actual SvcBusMessageInfo Api implementation 
* [x] deploy to AppService in test environment, could be manual from VS at first? (medium)
* [x] fix issue where scroll position not preserved and/or add pause updating button etc (small)
* [x] replacements should have refresh schedule defined in json, consider using SimpleScheduler? (small/medium)
* [x] server should provide Api of available resources (small)
* [x] frontend should list available resources in a menu/treeview? and/or define in route? (small)
* [x] lookup ResourceDocumentations from json/yaml file uri and provide API call to refresh? (medium)
* [x] Consider if a cache with item expiry would remove the need for a scheduler? only tasks which should do persistent storage should use the scheduler?
* [x] server should Push rather than frontend pull, web sockets/signalR etc? (large)
* [x] only query if someone is browsing? depend number of active clients, cache might help with this (?)
* [x] add stats endpoint with client connections and replacements stats etc (medium)
* [ ] get fragment routing working in front end (small) done needs testing
* [ ] get links across resourceDocs working (small)
* [ ] turn navmenu into a nested contents using fragments etc (medium)
* [ ] fix client side scroll issue when deployed to appservice (?)
* [ ] fix client side e.href.replace is not a function error (?)
* [ ] switch to managed identities when accessing resources
* [ ] resource config discovery, url to devops + walk repos? (large)
* [ ] more replacements API call, Elastic query, App Insights query? need use cases (medium)
* [ ] better styling (?)
* [ ] split dummy data creation out to seperate web app and improve - queue actual messages* (medium)
* [ ] Consider storing some data in app insights? where it would be queryable outside of this app, flag in replacement config?

