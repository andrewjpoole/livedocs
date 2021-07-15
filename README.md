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

* serve Blazor Wasm app from server
* add Azure AD auth
* add actual SvcBusMessageInfo Api implementation 
* split dummy data creation out to seperate web app and improve - queue actual messages*
* switch to spans for replacements for perf
* replacements should have refresh schedule defined in json, only query if someone is browsing?
* frontend should list available resources? and/or define in route?
* resource config discovery, url to devops + walk repos? or list of urls?
* frontend should have have menu/treeview of resources etc
* backend should Push rather than frontend pull, web sockets/signalR etc?
* switch frontend/fix issue where scroll position not preserved and/or pause updating etc
* more replacements API call, Elastic query, App Insights query?
* better styling

