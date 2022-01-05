#!/bin/bash
for i in {1..5}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_ppo_params_curr.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_zero_padding --run-id=build_zero_padding-"$i"
done
