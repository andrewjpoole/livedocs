
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

<<bacsdb.uspGetDay3SettlementOfTodayGroupedByTransactionType>>

<<bacsdb.uspGetDay3SettlementOfTodayGroupedByTransactionStatus>>

## Todays Std18 Files

<<TodaysStd18File>>

## Bacs Inbound Payments Architecture
    
```mermaid
flowchart TD;
    %% define nodes
    s[Bacs Scheme STS service]    
    si(swift infrastructure)
    sb("swift bacs <br /> TeamNarwhal")
    click sb https://cbinfrastructure.visualstudio.com/cbi/_git/swift_bacs _blank;
    ir(iso router)
    bpi2s(bacs payments inbound Day 2 Splitter)
    bpi2(bacs payments inbound Day 2 Processor)
    bpi3(bacs payments inbound Day 3 Processor)
    bm(bacs mandates)
    a(accounts)
    ss(settlement service)
    wh(webhooks)
    bdb[(bacs database)]
    
    %% define queues with clickable links
    q1{{<<bacs-swift-inbound-payment-requests>>}}
    click q1 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-swift-inbound-payment-requests/overview" _blank;
    q2{{<<iso-router-inbound-payment-request>>}} 
    click q2 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/iso-router-inbound-payment-request/overview" _blank;   
    %%q3{{<<bacs-outbound-batched-messages>>}}
    %%click q3 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-outbound-batched-messages/overview" _blank;
    %%q4{{<<swift-send-message-request-queue>>}}
    %%click q4 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/swift-send-message-request-queue" _blank;
    q10{{<<inbound-bacs-pain009>> <br />AUDIS setup}}
    click q10 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-pain009" _blank;
    q11{{<<inbound-bacs-pain011>> <br />AUDIS cancel}}
    click q11 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-pain011" _blank;
    q12{{<<inbound-bacs-pacs008>> <br />credit}}
    click q12 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-pacs008" _blank;
    q13{{<<inbound-bacs-pacs003>> <br />debit}}
    click q13 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-pacs003" _blank;
    q14{{<<inbound-bacs-pacs004>> <br />return}}
    click q14 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-pacs004" _blank;
    
    q20{{<<bacs-split-direct-credit>>}}
    click q20 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-split-direct-credit" _blank;
    q21{{<<bacs-split-credit-contra>>}}
    click q21 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-split-credit-contra" _blank;
    q22{{<<bacs-split-direct-debit>>}}
    click q22 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-split-direct-debit" _blank;
    q23{{<<bacs-split-debit-contra>>}}
    click q23 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-split-debit-contra" _blank;
    q24{{<<inbound-bacs-new-mandate>>}}
    click q24 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-new-mandate" _blank;
    q25{{<<inbound-bacs-mandate-migration>>}}
    click q25 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/inbound-bacs-mandate-migration" _blank;
    q30{{<<bacs-process-pending-credit-transaction>>}}
    click q30 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-process-pending-credit-transaction/overview" _blank;
    q31{{<<bacs-process-pending-debit-transaction>>}}
    click q31 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-process-pending-debit-transaction/overview" _blank;
    q32{{<<bacs-process-pending-credit-transaction-contra>>}}
    click q32 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-process-pending-credit-transaction-contra/overview" _blank;
    q33{{<<bacs-process-pending-debit-transaction-contra>>}}
    click q33 "https://portal.azure.com/#@cbinfrastructure.com/resource/subscriptions/0df249d7-9fde-4cd0-a580-9bf84a4406a4/resourceGroups/cbuk-core-testnarwhal-servicebus-uksouth/providers/Microsoft.ServiceBus/namespaces/cbuk-core-testnarwhal-servicebus-uksouth/queues/bacs-process-pending-debit-transaction-contra/overview" _blank;
    %% define links with queue nodes
    s<-->|Soap|si
    si-->q1; q1-->sb
    %%sb-->q4; q4-->si
    sb-->q2; q2-->ir
    ir-->q10; q10-->bpi2s
    ir-->q11; q11-->bpi2s
    ir-->q12; q12-->bpi2s
    ir-->q13; q13-->bpi2s
    ir-->q14; q14-->bpi2s
    bpi2s-->q20; q20-->bpi2
    bpi2s-->q21; q21-->bpi2
    bpi2s-->q22; q22-->bpi2
    bpi2s-->q23; q23-->bpi2
    bpi2s-->q24; q24-->bm
    bpi2s-->q25; q25-->bm    
    bpi2-->q30; q30-->bpi3
    bpi2-->q31; q31-->bpi3
    bpi2-->q32; q32-->bpi3
    bpi2-->q33; q33-->bpi3
    bpi2-->bdb
    bpi3-->bdb
    bm-->bdb
    bpi3-->a
    bpi3-->ss
    ss-->wh
    %%bpi3-->q3; q3-->sb;
    
    %% define styles
    classDef externalService fill:#ffd,stroke:#333,stroke-width:2px
    classDef otherTeamService fill:#cad,stroke:#333,stroke-width:2px
    classDef narwhalService fill:#aad,stroke:#333,stroke-width:2px

    class s externalService
    class wh,si,ss,a otherTeamService
    class ir,sb,bpi2s,bpi2,bpi3,bm narwhalService
```

```mermaid
pie
"Dogs" : 386
"Cats" : 85
"Rats" : 15
```
