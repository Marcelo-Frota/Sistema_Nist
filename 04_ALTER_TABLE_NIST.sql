-- =================================================================================
-- SCRIPT DE ATUALIZAÇÃO DO BANCO DE DADOS
-- Implementação do Padrão NIST CSF 2.0 (Target Profile vs Current Profile)
-- =================================================================================

-- 1. Remover colunas obsoletas e suas regras (Constraints)
ALTER TABLE SGSI_AVALIA_SUBCAT DROP COLUMN tx_status_maturidade CASCADE CONSTRAINTS;
ALTER TABLE SGSI_AVALIA_SUBCAT DROP COLUMN tx_prioridade CASCADE CONSTRAINTS;
ALTER TABLE SGSI_AVALIA_SUBCAT DROP COLUMN nr_nivel_implementacao CASCADE CONSTRAINTS;
ALTER TABLE SGSI_AVALIA_SUBCAT DROP COLUMN tx_justificativa CASCADE CONSTRAINTS;

-- 2. Renomear coluna existente para ficar clara que pertence ao Alvo
ALTER TABLE SGSI_AVALIA_SUBCAT RENAME COLUMN tx_responsabilidades TO tx_responsabilidades_alvo;

-- 3. Adicionar as Novas Colunas (Alvo e Atual)
ALTER TABLE SGSI_AVALIA_SUBCAT ADD (
    -- Campos Target (Alvo)
    tx_prioridade_alvo VARCHAR2(50),
    tx_tier_alvo VARCHAR2(50),
    tx_referencias_alvo CLOB,
    
    -- Campos Current (Atual)
    tx_prioridade_atual VARCHAR2(50),
    tx_status_atual VARCHAR2(50),
    tx_politicas_atual CLOB,
    tx_praticas_atual CLOB,
    tx_responsabilidades_atual CLOB,
    tx_referencias_atual CLOB,
    tx_evidencias_atual CLOB
);

COMMIT;
