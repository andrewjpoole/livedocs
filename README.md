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
* [ ] deploy to AppService in test environment, could be manual from VS at first? (medium)
* [ ] switch to Spans for replacements for perf (small)
* [ ] fix issue where scroll position not preserved and/or add pause updating button etc (small)
* [x] replacements should have refresh schedule defined in json, consider using SimpleScheduler? (small/medium)
* [ ] server should provide Api of available resources (small)
* [ ] frontend should list available resources in a menu/treeview? and/or define in route? (small)
* [ ] resource config discovery, url to devops + walk repos? or lookup from json/yaml file uri? (medium)
* [ ] server should Push rather than frontend pull, web sockets/signalR etc? (large)
* [ ] only query if someone is browsing? depend number of active clients (?)
* [ ] more replacements API call, Elastic query, App Insights query? (medium)
* [ ] better styling (?)
* [ ] split dummy data creation out to seperate web app and improve - queue actual messages* (medium)
* [ ] Consider storing some data in app insights? where it would be queryable outside of this app, flag in replacement config?
* [ ] Consider if a cache with item expiry remove the need for a scheduler?

