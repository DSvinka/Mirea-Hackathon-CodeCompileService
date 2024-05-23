interface GetListDockerImageRequest {
  connectionId: string
}


interface DockerImageActionModel {
  action: string
  imageId: number
}

interface DockerImageModel {
  id: string;

  displayName: string;
  description: string;

  codeFileExtension: string;
  codeEditorLang: string;

  maxCountByUser: string;
}
