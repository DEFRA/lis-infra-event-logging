#!/bin/bash

# S3 buckets
aws s3 mb s3://event-logging-artefacts

# SQS queues
aws sqs create-queue \
  --queue-name event-submission-dlq \
  --attributes '{"MessageRetentionPeriod":"1209600"}'

DLQ_URL=$(aws sqs get-queue-url \
  --queue-name event-submission-dlq \
  --query 'QueueUrl' \
  --output text)

DLQ_ARN=$(aws sqs get-queue-attributes \
  --queue-url "${DLQ_URL}" \
  --attribute-names QueueArn \
  --query 'Attributes.QueueArn' \
  --output text)

aws sqs create-queue \
  --queue-name event-submission \
  --attributes "{\"VisibilityTimeout\":\"120\",\"ReceiveMessageWaitTimeSeconds\":\"20\",\"RedrivePolicy\":\"{\\\"deadLetterTargetArn\\\":\\\"${DLQ_ARN}\\\",\\\"maxReceiveCount\\\":\\\"5\\\"}\"}"
