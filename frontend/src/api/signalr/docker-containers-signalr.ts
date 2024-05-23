import signalR from "@microsoft/signalr";
import {config} from "../index"
import {useDockerStore} from "../../stores/docker";

import {DockerContainerActionModel, DockerContainerModel} from "../models/docker-container-models";
import {ErrorModel} from "../models/error-models";

const store = useDockerStore();
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${config.wsUrl}`)
  .build();

connection.on("docker-containers-update", (data: DockerContainerModel) => {
  store.updateContainer(data)
});

connection.on("docker-images-update", (data: DockerImageModel) => {
  store.updateImage(data)
});

connection.on("docker-success", (data: DockerContainerActionModel | DockerImageActionModel | ErrorModel) => {
  if ((data as DockerContainerActionModel).containerId) {
    data = data as DockerContainerActionModel;
    if (data.action === "delete") {
      store.removeContainer(data.containerId)
    } else if (data.action === "create") {
      store.sendGetContainers({connectionId: connection.connectionId})
    }
  }

  else if ((data as DockerImageActionModel).imageId) {
    data = data as DockerContainerActionModel;
    if (data.action === "delete") {
      store.removeImage(data.containerId)
    } else if (data.action === "create") {
      store.sendGetContainers({connectionId: connection.connectionId})
    }
  }

  store.disableLoading()
});

connection.on("docker-error", (data: ErrorModel) => {
  store.disableLoading()
  console.log(data.message)
});

export {connection};
