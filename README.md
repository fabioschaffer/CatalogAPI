Criar migration do EF:
	dotnet ef migrations add cria-tabelas-catalogAPI --project .\Infrastructure\Infrastructure.csproj --startup-project .\CatalogAPI\CatalogAPI.csproj

Executar a migration do EF:
	dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\CatalogAPI\CatalogAPI.csproj
	
Para rodar no docker, alterar no appsettings, na string de conexão ao MySql:
	server=host.docker.internal
	
Criar imagem da api para Docker:
	docker build -t catalog-api:1.0 .
	
Executar imagem:
	docker run -p 8081:8080 catalog-api:1.0
	
Abrir a aplicação:
	http://localhost:8081/swagger/index.html

## Pipeline Azure DevOps

Selecionar `Build, criar imagem e enviá-la ao acr.yml` como pipeline deste repositório.
Assim como na UsersAPI, alterações na `master` criam e publicam a imagem
`fiaptechchallenge.azurecr.io/catalogapi` com as tags do build e `latest`.
O estágio Deploy atualiza o container `catalog-api` do deployment
`catalog-api`, no namespace `fcg`, aguarda o rollout e verifica
se o endpoint de saúde retorna `Health: OK`.

Antes de executar, configurar:

- Autorizar a conexão ACR `dockerRegistryServiceConnection` para este pipeline.
- Criar/autorizar a conexão Azure `azure-aks-catalogapi` com acesso ao AKS
  `cluster-fiap`, no resource group `geral`, ou ajustar essas variáveis no YAML.
- Criar o Environment `catalogapi-aks-fcg` com a verificação **Exclusive lock**.
- Substituir `CONFIGURAR-CATALOGAPI-HEALTH-URL` em `catalogApiHealthUrl` pela URL
  acessível ao agente que termina em `/api/health`. A rota `/catalog` do Kong
  atualmente exige JWT; o script usa uma verificação sem autenticação, portanto
  essa URL precisa permitir o acesso ao endpoint de saúde.
- Garantir que o deployment já exista e que o AKS tenha permissão de pull no ACR.

O script `scripts/deploy-aks.sh` impede a implantação de um build mais antigo
que o atual e remove o arquivo temporário de credenciais ao finalizar.

## Indexação de jogos no Elasticsearch

Ao criar ou editar um jogo pela CatalogAPI, o serviço salva no MySQL, invalida
os caches existentes e aguarda a indexação no Elasticsearch antes de retornar.
O documento usa o ID do jogo (`PUT catalog-games/_doc/{id}`), com os campos
`id`, `nome` e `price`; edições substituem o mesmo documento sem duplicá-lo.
`refresh=wait_for` aguarda a disponibilidade do documento nas buscas.

Configuração em `CatalogAPI/appsettings.json` ou por variáveis de ambiente:

- `Elasticsearch__Url`: endereço do serviço (padrão: `https://my-elasticsearc`).
- `Elasticsearch__IndexName`: índice de destino (padrão: `catalog-games`).
- `Elasticsearch__ApiKey`: valor `encoded` da API key do Elasticsearch (Base64 de `id:api_key`), sem o prefixo `ApiKey`.
- `Elasticsearch__Username` e `Elasticsearch__Password`: alternativa para autenticação Basic com usuário e senha.

Forneça as credenciais por variáveis de ambiente ou User Secrets. Quando ambos
os métodos estiverem configurados, a API key tem prioridade. Sem credenciais,
a requisição é anônima e clusters protegidos retornam 401. Reinicie a API após
alterar a configuração. Um erro 401 requer verificar as credenciais; o JWT da
CatalogAPI não é uma credencial do Elasticsearch.

O índice precisa existir ou o cluster deve permitir sua criação automática.
As credenciais precisam permitir escrita no índice (e criação, se necessária).
Não há fila nem novas tentativas: assume-se que o Elasticsearch esteja disponível.
Uma falha HTTP é propagada e não desfaz a gravação já concluída no MySQL.
Não são indexadas alterações feitas diretamente por SQL nem jogos antigos
que não tenham sido criados/editados pela API. Exclusões não fazem parte deste fluxo.

Testes: `dotnet test Tests/Tests.csproj`.

## Busca de jogos

`GET /search?q=mariio&size=20`, com o token JWT no header `Authorization: Bearer ...`.
A busca consulta o campo `nome` no Elasticsearch usando `match` com
`fuzziness: AUTO` e ordena por `_score` decrescente (mais relevante primeiro).
A tolerância depende do comprimento de cada termo: até 2 caracteres não há
edições; de 3 a 5, até uma edição; acima disso, até duas.

A resposta é uma lista de objetos com `id`, `nome`, `price` e `score`.
Sem correspondências, retorna `200` com `[]`. O parâmetro `q` é obrigatório
(1 a 200 caracteres após remover espaços das pontas); `size` é opcional,
com padrão 20 e limite de 1 a 100. Parâmetros inválidos retornam `400`.
A busca usa a mesma configuração e credencial Elasticsearch da indexação;
a credencial deve ter permissão de leitura no índice. O campo `nome` deve
estar mapeado como `text` (padrão para documentos indexados automaticamente).
