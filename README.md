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

* resource config discovery, url to devops + walk repos? or list of urls?
* frontend should list available resources? and/or define in route?
* backend should Push rather than frontend pull, web sockets/signalR etc?
* replacements should have refresh schedule defined in json
* switch to spans for replacements
* switch frontend/fix issue where scroll position not preserved
* more replacements API call, Elastic query, App Insights query?
* better styling

