#!/usr/bin/env sh

grpcurl -plaintext -d '{"name": "Rob"}' -proto src/Protosweeper.Api/Protos/greet.proto localhost:5170 greet.Greeter/SayHello
