aws cloudformation deploy `
    --profile=default `
    --stack-name=sescollector `
    --parameter-overrides `
        CodeBucketName=m2-codedeploy-bucket-2f95titbgkfk `
        ApiArtifactKey=SesCollector/SesCollector-Api-2022-05-22-23-10.zip `
        CollectorArtifactKey=SesCollector/SesCollector-Collector-2022-05-22-23-10.zip `
    --tags `
        'Platform=SesCollector' `
        'Environment=Dev' `
    --capabilities CAPABILITY_IAM `
    --template-file SesCollectorStack.yaml
