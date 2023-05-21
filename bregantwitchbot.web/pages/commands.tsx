import { DoGet } from '@/helpers/webFetchHelper';
import { Container, Text, TextInput } from '@mantine/core';
import { useDebouncedValue } from '@mantine/hooks';
import { IconSearch } from '@tabler/icons-react';
import { sortBy } from 'lodash';
import { DataTableSortStatus, DataTable } from 'mantine-datatable';
import { GetServerSideProps } from 'next';
import { useState, useEffect } from 'react';

interface CommandsProps {
    data:  GetCommandsDto[] | null;
    error: string | null;
}

interface GetCommandsDto {
    commandName: string;
    commandText: string;
    lastUsed:    string;
    timesUsed:   number;
}

const Commands = (props: CommandsProps) => {
    const PAGE_SIZES = [10, 25, 50];
    
    const [sortStatus, setSortStatus] = useState<DataTableSortStatus>({ columnAccessor: 'name', direction: 'asc' });
    const [pageSize, setPageSize] = useState(PAGE_SIZES[1]);
    const [page, setPage] = useState(1);
    const [records, setRecords] = useState(props.data?.slice(0, pageSize));
    const [query, setQuery] = useState('');
    const [debouncedQuery] = useDebouncedValue(query, 200);

    useEffect(() => {
        const from = (page - 1) * pageSize;
        const to = from + pageSize;
        setRecords(props.data?.slice(from, to));

        const data = sortBy(props.data, sortStatus.columnAccessor);
        const sortedData = sortStatus.direction === 'desc' ? data.reverse() : data;
        setRecords(sortedData);

        let filteredGames = sortedData.filter(({commandName, commandText}) => {
            if(
                debouncedQuery !== '' &&
                !`${commandName} ${commandText}`
                    .toLowerCase()
                    .includes(debouncedQuery.trim().toLowerCase())
            ) {
                return false;
            }

            return true;
        });

        setRecords(filteredGames.slice(from, to));

      }, [page, props.data, pageSize, sortStatus.columnAccessor, sortStatus.direction, debouncedQuery]);
      
    return ( 
        <>
            <Text size={60} weight={500} align='center' pb={30}>Commands</Text>

            {props.error && 
                <Text>{props.error}</Text>
            }
            
            {props.data &&
            <Container size="lg">

                <TextInput
                    sx={{ flexBasis: '60%' }}
                    placeholder="Search users..."
                    icon={<IconSearch size={16} />}
                    value={query}
                    onChange={(e) => setQuery(e.currentTarget.value)}
                />
                
                <DataTable 
                    withBorder
                    records={records}
                    columns={[
                        {accessor: 'commandName', title: 'Command Name', sortable: true},
                        {accessor: 'commandText', title: 'Command Text'},
                        {accessor: 'lastUsed', title: 'Last Used' },
                        {accessor: 'timesUsed', title: 'Times Used', sortable: true},
                    ]}
                    totalRecords={props.data.length}
                    paginationColor="grape"
                    recordsPerPage={pageSize}
                    page={page}
                    onPageChange={(p) => setPage(p)}
                    recordsPerPageOptions={PAGE_SIZES}
                    onRecordsPerPageChange={setPageSize}
                    sortStatus={sortStatus}
                    onSortStatusChange={setSortStatus}
                />
            </Container>
            }
        </>
     );
}

export const getServerSideProps: GetServerSideProps<CommandsProps> = async() => {

    try {
        const apiRes = await DoGet('/api/Commands/GetCommands');

        if(apiRes.ok) {
            return {
                props: {
                    data: await apiRes.json(),
                    error: null
                }
            }
        }
        else{
            return {
                props: {
                    data: null,
                    error: `Error getting data from API - ${apiRes.status}`
                }
            }
        }
    } catch (error) {
        return {
            props: {
                data: null,
                error: `Error getting data from API - ${error}`
            }
        }
    }
}

export default Commands;