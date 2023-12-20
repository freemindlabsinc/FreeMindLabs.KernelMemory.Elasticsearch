# How to install a running version of Elasticsearch and Kibana 8.x running on Docker

This article will show you how to install a running version of Elasticsearch and Kibana 8.x running on Docker.
The data will be persisted in a volume on the host machine, so it can survive container restart.

These are the steps we will follow:
1. Make sure that the value of `vm.max_map_count` on the Docker host machine is high enough.
1. Create a Docker network called `elastic`.
    1. We will use this network to connect Elasticsearch and Kibana.
1. We will then launch Elasticsearch using Docker.
    1. When Elasticsearch runs for the first time it will output some security keys that we need to grab. 
    1. Such keys will allow us to later connect Kibana to Elasticsearch.
1. We will then launch Kibana using Docker.
    1. We will use the keys we grabbed from the logs of Elasticsearch to connect Kibana to Elasticsearch.
1. The installation should be now complete. 

To access Kibana you should be able to point your browser at http://localhost:5601
Kibana will, in turn, connected to Elasticsearch at http://localhost:9200

## Prerequisites

1. WSL2 running properly, if on Windows
1. Docker running on Linux/WSL2

## Launch a terminal to configure the Docker host

Launch a terminal and go to the '\docker' subfolder of this repo, which is where we will mount the volumes that will contain the data of Elasticsearch and Kibana. *We don't necessarily need in this folder to run all the commands, but it is a good practice to keep all the files related to a project in the same folder.*

If you are in a Powershell terminal, just go to the ```\docker``` subfolder of this repository.

```powershell
PS D:\> cd .\FreeMindLabs.KernelMemory.Elasticsearch\docker\
```

Then launch WSL by typing ```wsl``` in the terminal

```powershell
PS D:\FreeMindLabs.KernelMemory.Elasticsearch\docker> wsl
```

You should now be in a Linux terminal similar to the following:

```bash
sysadmin@OptimusPrime:/mnt/d/FreeMindLabs.KernelMemory.Elasticsearch/docker$
```

## How to Install Elasticsearch

Before we can launch a Docker container running Elasticsearch, we need to configure two things:

1. Set a correct value for `vm.max_map_count` on the Docker host.
1. Create a Docker network called `elastic` that we will use to connect Elasticsearch and Kibana.

### Set vm.max_map_count on the Docker host

> vm.max_map_count is a critical setting in Linux systems, particularly necessary for running Elasticsearch efficiently. It defines the maximum number of memory map areas a process can have. 
> The default vm.max_map_count value is typically too low for Elasticsearch, potentially leading to performance issues or even **preventing it from starting**. By increasing this value to at least 262144, as recommended by Elasticsearch's official documentation, you ensure that the Elasticsearch process has sufficient memory map areas for optimal performance and stability.

To see the current value of `vm.max_map_count`, run the following command from WSL/Linux:

```bash
sysctl vm.max_map_count
```

If the value you read is less than `262144`, you need to increase it.

You can set `vm.max_map_count` on the Docker host machine in two ways: temporarily or permanently.

#### Temporarily (until the next reboot):
Run the following command on your host machine (not inside the container):

```bash
sudo sysctl -w vm.max_map_count=262144
```

This command sets `vm.max_map_count` to `262144` temporarily.

#### Permanently (reccomended):
To make this change permanent, you need to add it to your system's configuration file.

1. Edit the `/etc/sysctl.conf` file on your host machine:

    ```bash
    sudo nano /etc/sysctl.conf
    ```

2. Add the following line at the end of the file:

    ```
    vm.max_map_count=262144
    ```

3. Save and close the file.

4. To apply the changes without rebooting, run:

    ```bash
    sudo sysctl -p
    ```

### Create a Docker network called `elastic`

To create a Docker network called `elastic`, run the following command from WSL/Linux:

```bash
docker network create elastic
```

The response should be something like this:

```bash
8a42de666bc5fdc5de1b9951eddc31da059eb2a13eb2c9eec879c7b0d9a0906b
```

### Launch Elasticsearch

To launch Elasticsearch, run the following command from WSL/Linux:

```bash
 docker run -d --name elasticsearch-01 --net elastic -p 9200:9200 -e "discovery.type=single-node" -v $(pwd)/elasticsearch:/usr/share/elasticsearch/data -m 4g docker.elastic.co/elasticsearch/elasticsearch:8.3.3
```
The response should be something like this:

```bash
a4c20c2ecbec1fc0f90d72e481924928e4af49a16464928d92d0fedba784eb54
```

Now we need to inspect the logs so we can grab the keys we need.
Run the following command from WSL/Linux:

```bash
docker logs elasticsearch-01 -f
```

At some point, after everything takes the time it needs, you should see the following:

```bash

```


## References

1. [How to install elasticsearch and kibana 8.x using Docker?](https://www.devopsschool.com/blog/how-to-install-elasticsearch-and-kibana-using-docker/)
1. [Install Elasticsearch with Docker](https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html)
1. [Install Kibana with Docker](https://www.elastic.co/guide/en/kibana/current/docker.html)
1. [Deploying Elasticsearch and Kibana with Docker](https://quoeamaster.medium.com/deploying-elasticsearch-and-kibana-with-docker-86a4ac78d851)
1. (Getting started with the Elastic Stack and Docker Compose: Part 1)[https://www.elastic.co/blog/getting-started-with-the-elastic-stack-and-docker-compose]
1. 