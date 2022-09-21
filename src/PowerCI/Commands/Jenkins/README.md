# Jenkins tools

## Usage

- Install jenkins

``` bash
./power-ci jenkins install
```

- Set config

``` bash
./power-ci jenkins set host {your jenkins host like http://www.jenkins.com}
./power-ci jenkins set user {your jenkins user like admin}
./power-ci jenkins set password {your jenkins password or token}
```

- Get current config

``` bash
./power-ci jenkins get
```

- Handle jenkins folder

``` bash
# List jenkins folders
./power-ci jenkins folder list

# Create jenkins folder
./power-ci jenkins folder create -n {folder name}
```

- Handle jenkins job

``` bash
# List jenkins jobs
./power-ci jenkins job list

# List jenkins jobs of specific folder
./power-ci jenkins job list -f {job name}

# Create job
./power-ci jenkins job create -n {job name} -t freestyle
./power-ci jenkins job create -n {job name} -t pipeline
./power-ci jenkins job create -n {job name} -t multibranch
./power-ci jenkins job create -n {job name} -t freestyle -f {folder name}

# Copy job
./power-ci jenkins job copy -s {job} -t {new job}
./power-ci jenkins job copy -s {job} -t {new folder/new job}
./power-ci jenkins job copy -s {folder/job} -t {new job}
./power-ci jenkins job copy -s {folder/job} -t {new folder/new job}
./power-ci jenkins job copy -s {job} -t {new job} -g {your new git repo address}
```
