-- Script de Carga Completa - Categorias e Subcategorias Principais do NIST CSF 2.0
-- Apaga os dados antigos para evitar duplicidade (caso tenha rodado o script anterior)
DELETE FROM SGSI_SUBCATEGORIA;
DELETE FROM SGSI_CATEGORIA;

-- ==========================================
-- GOVERNAR (GV) - Contexto Organizacional
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'GV'), 'GV.OC', 'Contexto Organizacional', 'As circunstâncias da organização (missão, expectativas, dependências, requisitos legais) são compreendidas e informam as decisões de gerenciamento de riscos.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-01', 'A missão organizacional é compreendida e informa o gerenciamento de riscos de cibersegurança.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-02', 'As partes interessadas internas e externas são compreendidas e suas necessidades consideradas.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-03', 'Os requisitos legais, regulatórios e contratuais são compreendidos e gerenciados.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-04', 'Os objetivos críticos, capacidades e serviços que dependem da organização são comunicados.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-05', 'Os resultados, capacidades e serviços dos quais a organização depende são compreendidos.', 'ATIVO');

-- ==========================================
-- GOVERNAR (GV) - Estratégia de Gestão de Riscos
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'GV'), 'GV.RM', 'Estratégia de Gestão de Riscos', 'As prioridades da organização, tolerância e apetite a riscos são estabelecidos e comunicados.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.RM'), 'GV.RM-01', 'Os objetivos de gerenciamento de riscos são estabelecidos e acordados.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.RM'), 'GV.RM-02', 'O apetite e a tolerância a riscos são estabelecidos e comunicados.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.RM'), 'GV.RM-03', 'O risco de cibersegurança é integrado ao gerenciamento de riscos corporativos.', 'ATIVO');


-- ==========================================
-- IDENTIFICAR (ID) - Gestão de Ativos
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'ID'), 'ID.AM', 'Gestão de Ativos', 'Os dados, pessoal, dispositivos, sistemas e instalações são identificados e gerenciados.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'ID.AM'), 'ID.AM-01', 'Os inventários de hardware são mantidos.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'ID.AM'), 'ID.AM-02', 'Os inventários de software e serviços são mantidos.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'ID.AM'), 'ID.AM-03', 'As representações da arquitetura de rede da organização são mantidas.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'ID.AM'), 'ID.AM-04', 'Os ativos de dados e informações são identificados e gerenciados.', 'ATIVO');


-- ==========================================
-- PROTEGER (PR) - Controle de Acesso
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'PR'), 'PR.AA', 'Gerenciamento de Identidade e Acesso', 'O acesso a ativos físicos e lógicos é limitado a usuários autorizados.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'PR.AA'), 'PR.AA-01', 'As identidades e credenciais são gerenciadas.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'PR.AA'), 'PR.AA-02', 'A autenticação de usuários é gerenciada.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'PR.AA'), 'PR.AA-03', 'As permissões de acesso lógico são gerenciadas (Menor Privilégio).', 'ATIVO');


-- ==========================================
-- DETECTAR (DE) - Monitoramento Contínuo
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'DE'), 'DE.CM', 'Monitoramento Contínuo', 'As redes e o ambiente físico são monitorados para encontrar anomalias.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'DE.CM'), 'DE.CM-01', 'As redes e o ambiente físico são monitorados de forma contínua.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'DE.CM'), 'DE.CM-02', 'A atividade maliciosa e conexões não autorizadas são detectadas.', 'ATIVO');


-- ==========================================
-- RESPONDER (RS) - Análise e Mitigação
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'RS'), 'RS.MA', 'Análise e Mitigação', 'As atividades são realizadas para analisar e mitigar os efeitos de um incidente.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'RS.MA'), 'RS.MA-01', 'As notificações de sistemas de detecção são investigadas.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'RS.MA'), 'RS.MA-02', 'O impacto de um incidente é compreendido.', 'ATIVO');


-- ==========================================
-- RECUPERAR (RC) - Planejamento
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'RC'), 'RC.RP', 'Planejamento de Recuperação', 'Os processos de recuperação são executados.');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'RC.RP'), 'RC.RP-01', 'O plano de recuperação é executado durante ou após um incidente.', 'ATIVO');

COMMIT;
