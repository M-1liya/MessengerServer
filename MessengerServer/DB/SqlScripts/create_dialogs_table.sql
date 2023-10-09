CREATE OR REPLACE FUNCTION create_dialogs_table(target_table_name TEXT, max_value INT)
RETURNS VOID AS $$
DECLARE
    i INT := max_value - 998;
BEGIN

EXECUTE 'CREATE TABLE IF NOT EXISTS ' || target_table_name || ' (number INT, content TEXT, participants VARCHAR(1000))';
    WHILE i <= max_value LOOP
        EXECUTE 'INSERT INTO ' || target_table_name || ' (number) VALUES ($1)' USING i;
        i := i + 1;
    END LOOP;
END;
$$ LANGUAGE plpgsql;

-- Вызовите функцию, чтобы добавить числа от 1 до 999 в таблицу
SELECT insert_numbers();

select * from dialogs1_999;

DELETE FROM dialogs1_999;

SELECT create_dialogs_table('dialogs1001_1999', 1999);

SELECT * FROM dialogs1001_1999;
