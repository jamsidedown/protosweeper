#!/usr/bin/env sh

grpcurl -plaintext -d '{"name": "Rob"}' -proto src/Protosweeper.Web/Protos/greet.proto localhost:5170 greet.Greeter/SayHello
