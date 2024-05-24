export interface GetListDockerImageRequest {
  connectionId: string
}


export interface DockerImageActionModel {
  action: string
  imageId: number
}

export interface DockerImageModel {
  id: string;

  displayName: string;
  description: string;

  codeFileExtension: string;
  codeEditorLang: string;

  maxCountByUser: string;
}
