#!/bin/bash
for i in {1..5}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher_ppo_params_curr_sparse_reward.yaml --base-port 38278 --no-graphics --env=Project/Builds/build_next_target_id_sparse_reward --run-id=build_next_target_id_sparse_reward-"$i"
done
