#!/bin/bash
for i in {4..4}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_const_curr_0.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_18 --run-id=build_18_run-"$i"
done
