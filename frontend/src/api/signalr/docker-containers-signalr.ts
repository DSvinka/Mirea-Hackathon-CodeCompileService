import signalR from "@microsoft/signalr";
import {config} from "../index"
import {useDockerStore} from "../../stores/docker";

import {DockerContainerActionModel, DockerContainerModel} from "../models/docker-container-models";
import {ErrorModel} from "../models/error-models";
import {DockerImageActionModel, DockerImageModel} from "../models/docker-image-models";
import {useSignalR} from "@quangdao/vue-signalr";

export function setupSignalR() {
  const signalr = useSignalR();
  const store = useDockerStore();


  signalr.on("docker-containers-update", (data: DockerContainerModel) => {
    store.updateContainer(data)
  });

  signalr.on("docker-images-update", (data: DockerImageModel) => {
    store.updateImage(data)
  });

  signalr.on("docker-success", (data: DockerContainerActionModel | DockerImageActionModel | ErrorModel) => {
    if ((data as DockerContainerActionModel).containerId) {
      data = data as DockerContainerActionModel;
      if (data.action === "delete") {
        store.removeContainer(data.containerId)
      } else if (data.action === "create") {
        store.sendGetContainers({connectionId: signalr.connection.connectionId})
      }
    }

    else if ((data as DockerImageActionModel).imageId) {
      data = data as DockerContainerActionModel;
      if (data.action === "delete") {
        store.removeImage(data.containerId)
      } else if (data.action === "create") {
        store.sendGetContainers({connectionId: signalr.connection.connectionId})
      }
    }

    store.disableLoading()
  });

  signalr.on("docker-error", (data: ErrorModel) => {
    store.disableLoading()
    console.log(data.message)
  });
}
