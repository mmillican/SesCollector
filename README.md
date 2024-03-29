# SES Event Collector

Allows you to send SES Events (bounces, deliveries, etc) to a DynamoDB table 
and provides an API to easily retrieve them in a usable format.

## Deploying

SES Collector is deployed via CloudFormation. Use the following to deploy:

```bash
$ aws cloudformation update-stack --stack-name SesCollector --template-body file://SesCollectorStack.yaml --parameters ParameterKey=CreateDeploymentBucket,ParameterValue=true,UsePreviousValue=false --capabilities CAPABILITY_AUTO_EXPAND
```

## TODO

- Write unit tests for additional event types
- Create a frontend UI to consume the API and provide way to view/search events
- Possibly add a daily digest email?
- [Add alarms](https://gist.github.com/tomislacker/cb36f6a2b699e4707840066cd9433ecb)
- Add option to frontend to get config from a file instead of ENV variables