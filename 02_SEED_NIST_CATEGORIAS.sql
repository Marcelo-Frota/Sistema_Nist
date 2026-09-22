-- Script de Carga Inicial - Categorias e Subcategorias do NIST CSF 2.0
-- Este script popula as tabelas para que os formulários de avaliação apareçam na tela do Cenário Alvo.

-- ==========================================
-- 1. CATEGORIAS DA FUNÇÃO 'GOVERNAR' (GV)
-- ==========================================
INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'GV'), 'GV.OC', 'Contexto Organizacional', 'As circunstâncias - missão, expectativas das partes interessadas, dependências e requisitos legais - são compreendidas.');

INSERT INTO SGSI_CATEGORIA (id_funcao, cd_categoria, tx_nome, tx_descricao) 
VALUES ((SELECT id_funcao FROM SGSI_FUNCAO WHERE cd_funcao = 'GV'), 'GV.RM', 'Estratégia de Gestão de Riscos', 'As prioridades da organização, tolerância a riscos e declarações de apetite a riscos são estabelecidas e comunicadas.');

-- ==========================================
-- 2. SUBCATEGORIAS DE 'GV.OC' (Contexto Organizacional)
-- ==========================================
INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-01', 'A missão organizacional é compreendida e informa o gerenciamento de riscos de cibersegurança.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-02', 'As partes interessadas internas e externas são compreendidas, e suas necessidades e expectativas em relação à cibersegurança são consideradas.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.OC'), 'GV.OC-03', 'Os requisitos legais, regulatórios e contratuais relativos à cibersegurança são compreendidos e gerenciados.', 'ATIVO');

-- ==========================================
-- 3. SUBCATEGORIAS DE 'GV.RM' (Estratégia de Gestão de Riscos)
-- ==========================================
INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.RM'), 'GV.RM-01', 'Os objetivos de gerenciamento de riscos são estabelecidos e acordados pelas partes interessadas organizacionais.', 'ATIVO');

INSERT INTO SGSI_SUBCATEGORIA (id_categoria, cd_subcategoria, tx_descricao, tx_status)
VALUES ((SELECT id_categoria FROM SGSI_CATEGORIA WHERE cd_categoria = 'GV.RM'), 'GV.RM-02', 'Declarações de apetite e tolerância a riscos de cibersegurança são estabelecidas, mantidas e comunicadas.', 'ATIVO');

COMMIT;
