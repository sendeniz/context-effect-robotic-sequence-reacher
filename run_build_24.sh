#!/bin/bash
for i in {1..10}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_const_curr_0.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_24 --run-id=build_24_run-"$i"
done
