
# Bacs Domain Live Dashboard 8-)

Some documentation should go here: 
* general description of the system
* maybe SLAs
* some links to existing documentation or dashboards?
* info on servicebus queues including what a DL means, what to do about it and how long you have to do it?
* what issues should/should not be escalated out-of-hours
* link to the repo(s)

## Day 3 Processing scheduled transactions

paragraph about Bacs day 3 processing etc

<<livedocsdb.sp_get_day2_stats>>

```mermaid
graph TD
A[swift infrastructure] -->|<<bacs-swift-inbound-payment-requests>>| B(swift-bacs)
B -->|<<iso-router-inbound-payment-request>>| C{IsoRouter}
C -->|<<bacs-process-pending-credit-transaction>>| D[BacsInboundPayments]
C -->|<<bacs-process-pending-debit-transaction>>| D[BacsInboundPayments]
D -->|<<bacs-outbound-batched-messages>>| E[Accounts]
D -->|<<swift-send-message-request-queue>>| F[WebHooks]
```

```mermaid
pie
"Dogs" : 386
"Cats" : 85
"Rats" : 15
```
