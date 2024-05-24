import { defineStore } from 'pinia'

import {Docker, getAuthHeader,} from "../api"
import {ErrorModel} from "../api/models/error-models";
import {DockerContainerModel, GetListDockerContainerRequest} from "../api/models/docker-container-models";
import {DockerImageModel, GetListDockerImageRequest} from "../api/models/docker-image-models";

interface DockerState {
  containers: DockerContainerModel[]
  images: DockerImageModel[]

  connectionId?: string

  currentImageIndex?: number
  currentContainerIndex?: number;

  isLoading: boolean
}


export const useDockerStore = defineStore("docker", {
  state: (): DockerState => ({
    containers: [],
    images: [],

    connectionId: null,

    currentContainerIndex: null,
    currentImageIndex: null,

    isLoading: false
  }),

  getters: {
    getContainers(state: DockerState): DockerContainerModel[]{
      return state.containers
    },

    getImages(state: DockerState): DockerImageModel[]{
      return state.images
    },

    getConnectionId(state: DockerState): string{
      return state.connectionId
    },

    getCurrentContainerIndex(state: DockerState): number{
      return state.currentContainerIndex
    },

    getCurrentImageIndex(state: DockerState): number{
      return state.currentImageIndex
    },

    getIsLoading(state: DockerState): boolean{
      return state.isLoading
    },
  },

  actions: {
    async sendGetContainers(request: GetListDockerContainerRequest) {
      try {
        const response = await Docker.post<void | ErrorModel>("/containers/get/all/user", request, {headers: getAuthHeader()})
        if (response.status !== 200) {
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
        const response = await Docker.post<void | ErrorModel>("/images/get/all", request, {headers: getAuthHeader()})
        if (response.status !== 200) {
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


    enableLoading() {
      this.isLoading = true;
    },

    disableLoading() {
      this.isLoading = false;
    },


    setCurrentImageIndex(imageIndex?: number) {
      this.currentImageIndex = imageIndex;
    },

    setCurrentContainerIndex(containerIndex?: number) {
      this.currentContainerIndex = containerIndex;
    },

    setConnectionId(connectionId: number) {
      this.connectionId = connectionId
    }
  },
})
