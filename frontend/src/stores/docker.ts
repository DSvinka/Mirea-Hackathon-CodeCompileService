import { defineStore } from 'pinia'

import {Docker, getAuthHeader,} from "../api"
import {ErrorModel} from "../api/models/error-models";
import {DockerContainerModel, GetListDockerContainerRequest} from "../api/models/docker-container-models";

interface DockerState {
  containers: DockerContainerModel[]
  images: DockerImageModel[]

  currentImageIndex?: number
  currentContainerIndex?: number;

  isLoading: boolean
}


export const useDockerStore = defineStore("docker", {
  state: (): DockerState => ({
    containers: [],
    images: [],

    currentContainerIndex: null,
    currentImageIndex: null,

    isLoading: false
  }),

  getters: {
    getContainers(state: DockerState): DockerContainerModel[]{
      return state.containers
    }
  },

  actions: {
    async sendGetContainers(request: GetListDockerContainerRequest) {
      try {
        const response = await Docker.post<void | ErrorModel>("/containers", request, {headers: getAuthHeader()})
        if (response.status !== 202) {
          alert((response.data as ErrorModel).message)
        } else {
          this.isLoading = true;
        }
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },

    async sendGetImages(request: GetListDockerImageRequest) {
      try {
        const response = await Docker.post<void | ErrorModel>("/images", request, {headers: getAuthHeader()})
        if (response.status !== 202) {
          alert((response.data as ErrorModel).message)
        } else {
          this.isLoading = true;
        }
      }
      catch (error) {
        alert(error)
        console.log(error)
      }
    },


    updateContainer(model: DockerContainerModel) {
      const index = this.containers.findIndex((e: DockerContainerModel) => e.id === model.id);
      if (index) {
        this.containers[index] = model
      } else {
        this.containers.append(model)
      }
    },

    updateImage(model: DockerImageModel) {
      const index = this.containers.findIndex((e: DockerImageModel) => e.id === model.id);
      if (index) {
        this.images[index] = model
      } else {
        this.images.append(model)
      }
    },


    removeContainer(modelId: number) {
      this.containers = this.containers.filter((e: DockerContainerModel) => e.id !== modelId);
    },

    removeImage(modelId: number) {
      this.containers = this.containers.filter((e: DockerContainerModel) => e.id !== modelId);
    },


    disableLoading() {
      this.isLoading = false;
    }
  },
})
