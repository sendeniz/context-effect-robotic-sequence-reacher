#!/bin/bash
for i in {1..6}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_ppo_params_curr.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_next_target_id_9 --run-id=build_next_target_id_9_curr_minr6_decay995-"$i"
done
