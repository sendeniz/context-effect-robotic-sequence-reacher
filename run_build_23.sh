#!/bin/bash
for i in {4..10}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_const_curr_0.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_23 --run-id=build_23_run-"$i" --resume
done
