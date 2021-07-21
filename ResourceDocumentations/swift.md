
# Swift Domain Live Dashboard 8-)

Some documentation should go here: 
* general description of the system
* maybe SLAs
* some links to existing documentation or dashboards?
* info on servicebus queues including what a DL means, what to do about it and how long you have to do it?
* what issues should/should not be escalated out-of-hours
* link to the repo(s)

## Day 3 Processing scheduled transactions

paragraph about Bacs day 3 processing etc

<<bacsdb.uspGetDay3SettlementOfTodayGroupedByTransactionType>>

```mermaid
graph TD;    
    A-->B{{bacs-swift-inbound-payment-requests fa:fa-envelope 234 fa:fa-clock 23654 fa:fa-book-dead 0}};
    B-->C(rounded square);
    A---db;
    d[<<hello>>] 
    db[(Database)]
    click B "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-swift-inbound-payment-requests/overview" _blank
    style A fill:#aad,stroke:#333,stroke-width:2px
    style C fill:#aad,stroke:#333,stroke-width:2px
```

```mermaid
pie
"Dogs" : 386
"Cats" : 85
"Rats" : 15
```
