#!/bin/bash
for i in {7..10}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_const_curr_0.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_17 --run-id=build_17_run-"$i"
done
