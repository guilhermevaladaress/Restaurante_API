# Atualizacoes administrativas

## Cardapio

O admin agora pode criar pratos diretamente por periodo:

### Criar prato de almoco

```http
POST /api/cardapio/almoco
Authorization: Bearer {token-admin}
Content-Type: application/json
```

```json
{
  "nome": "File de Frango com Pure",
  "descricao": "Prato executivo com frango grelhado, pure de batata e legumes",
  "precoBase": 36.90
}
```

### Criar prato de jantar

```http
POST /api/cardapio/jantar
Authorization: Bearer {token-admin}
Content-Type: application/json
```

```json
{
  "nome": "Risoto de Cogumelos",
  "descricao": "Risoto cremoso com mix de cogumelos frescos e queijo parmesao",
  "precoBase": 49.90
}
```

Esses dois endpoints criam o item ja como ativo. Por isso, o prato aparece automaticamente em:

- `GET /api/cardapio?periodo=Almoco`
- `GET /api/cardapio?periodo=Jantar`

O endpoint antigo `POST /api/cardapio` continua disponivel para cenarios em que o admin queira informar o periodo manualmente.

### Upload de imagem do prato

O admin agora pode enviar uma imagem para cada item do cardapio. A API recebe `multipart/form-data`, converte o arquivo para base64 e grava no banco.

```http
PUT /api/cardapio/{itemId}/midia
Authorization: Bearer {token-admin}
Content-Type: multipart/form-data
```

Campos aceitos:

- `imagem`: arquivo de imagem (`image/jpeg`, `image/png`, `image/webp`, `image/gif`)
- `removerImagem`: `true` para apagar a imagem atual

Limites atuais:

- Imagem: ate 5 MB

Para o menu:

- `GET /api/cardapio?periodo=Almoco`
- `GET /api/cardapio?periodo=Jantar`

agora retornam tambem:

- `imagemBase64`
- `imagemMimeType`
- `possuiImagem`

Se a interface precisar abrir a midia completa de um prato especifico, use:

```http
GET /api/cardapio/{itemId}/midia
```

Esse endpoint retorna a imagem armazenada em base64 para o item informado.

## Seed inicial do cardapio

O projeto continua subindo com os 40 pratos padrao, mas agora o seed ficou idempotente.

Isso significa:

- Se a tabela estiver vazia, os 40 pratos sao criados normalmente
- Se alguns pratos ja existirem, os que faltam sao adicionados
- As descricoes padrao sao atualizadas automaticamente nos pratos base
- Os itens base voltam a ficar ativos se tiverem sido desativados
- Midias ja cadastradas manualmente nao sao apagadas pelo seed

As descricoes dos 40 pratos agora estao definidas no catalogo de seed em vez de copiar apenas o nome do prato.

## Reservas

O admin agora pode listar todas as reservas feitas pelos clientes:

```http
GET /api/reservas
Authorization: Bearer {token-admin}
```

Exemplo de resposta:

```json
[
  {
    "id": 1,
    "dataHora": "2026-05-20T12:30:00",
    "numerMesa": 10,
    "numeroPessoas": 2,
    "status": "Confirmada",
    "codigoConfirmacao": "ABC12345",
    "usuarioId": "user-id",
    "nomeCliente": "Maria Souza",
    "emailCliente": "maria@example.com"
  }
]
```

Rotas relacionadas:

- Cliente lista as proprias reservas em `GET /api/reservas/minhas`
- Cliente cancela reserva em `PATCH /api/reservas/{id}/cancelar`
- Admin lista todas as reservas em `GET /api/reservas`
