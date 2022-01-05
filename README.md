# Context Effects for Motion Optimization in Robotic Sequential Reaching
**General:**
This repo contains a fully articulated robotic arm controller solving a sequential reaching task, allowing for close comparison between how robots learn and previously observed human data on sequential reaching.
<p align="center">
  <img src="figs/reaching_example.gif" alt="animated" />
  <figcaption>Fig.1 - Animated example of the training environment.</figcaption>
</p>

**Training:**
<br>
To train and replicate results it is recommended to 1) create your own executable build within the Unity environment via the "**File/Build Setting**" tab and 2) create a corresponding shell "**.shl**" file to train from scratch. Please select the corresponding target platform you wish to run the executable one. An example for a shell file to train PPO is given below: 

```
your_shell_file.sh
```

```
#!/bin/bash
for i in {1..3}
do
   echo "Starting run $i"
   mlagents-learn config/ppo/Reacher.yaml --base-port 38278 --no-graphics --env=Project/Builds/your_build_no_1 --run-id=your_build_no_1_run-"$i"
done
```
Note that the **.yaml** file contains all hyperparameters for training.

Given that you have created your own build "**your_build_no_1**" and want to initiate training, start your concole, 1. activate your virtual environment, 2. go to your project folder, 3. ensure your shell file **your_shell_file.sh** callable and begin training by calling your shell file **your_shell_file.sh** as:

```
1. conda activate //anaconda3/envs/UnityML
2. cd /Users/last_name/project_location
3. chmod +x your_shell_file.sh
4. ./your_shell_file.sh
```
Note that for windows or linux the execution procedure may differ as this was tested on a mac os. 
