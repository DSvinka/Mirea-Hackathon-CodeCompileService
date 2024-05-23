export interface CreateDockerContainerRequest {
  connectionId: string
  imageId: number

  programCode: string
}

export interface DeleteDockerContainerRequest {
  connectionId: string

  containerId: number
}

export interface GetListDockerContainerRequest {
  connectionId: string
}


export interface DockerContainerActionModel {
  action: string
  containerId: number
}

export interface DockerContainerModel {
  id: number

  userId: number
  containerId: number

  status: number
  logs?: string

  programCode: string

  usageMemory: number
  usageCpu: number
  usageStorage: number
}
