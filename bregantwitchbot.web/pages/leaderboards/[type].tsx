import { DoGet } from "@/helpers/webFetchHelper";
import { GetServerSideProps } from "next";
import { Container, Text, TextInput } from '@mantine/core';
import { useEffect, useState } from "react";
import { DataTable, DataTableSortStatus } from "mantine-datatable";
import { useDebouncedValue } from "@mantine/hooks";
import { IconSearch } from "@tabler/icons-react";
import sortBy from 'lodash/sortBy';

interface LeaderboardProps {
    data:  GetLeaderboardDto | null;
    error: string | null;
}

interface GetLeaderboardDto {
    leaderboardName: string;
    leaderboards:    LeaderboardData[];
}

interface LeaderboardData {
    position: number;
    username: string;
    amount:   string;
}

const Leaderboards = (props: LeaderboardProps) => {
    const PAGE_SIZES = [10, 25, 50];
    
    const [sortStatus, setSortStatus] = useState<DataTableSortStatus>({ columnAccessor: 'name', direction: 'asc' });
    const [pageSize, setPageSize] = useState(PAGE_SIZES[1]);
    const [page, setPage] = useState(1);
    const [records, setRecords] = useState(props.data?.leaderboards.slice(0, pageSize));
    const [query, setQuery] = useState('');
    const [debouncedQuery] = useDebouncedValue(query, 200);

    useEffect(() => {
        const from = (page - 1) * pageSize;
        const to = from + pageSize;
        setRecords(props.data?.leaderboards.slice(from, to));

        const data = sortBy(props.data?.leaderboards, sortStatus.columnAccessor);
        const sortedData = sortStatus.direction === 'desc' ? data.reverse() : data;
        setRecords(sortedData);

        let filteredGames = sortedData.filter(({username}) => {
            if(
                debouncedQuery !== '' &&
                !`${username}`
                    .toLowerCase()
                    .includes(debouncedQuery.trim().toLowerCase())
            ) {
                return false;
            }

            return true;
        });

        setRecords(filteredGames.slice(from, to));

      }, [page, props.data?.leaderboards, pageSize, sortStatus.columnAccessor, sortStatus.direction, debouncedQuery]);

    return ( 
        <>
            {props.error && 
                <Text>{props.error}</Text>
            }
            
            {props.data &&
            <Container size="lg">
                <Text size={60} weight={500} align='center' pb={30}>{props.data.leaderboardName}  Leaderboard</Text>

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
                        {accessor: 'position', title: 'Position' ,sortable: true},
                        {accessor: 'username', title: 'Username'},
                        {accessor: 'amount', title: 'Amount' },
                    ]}
                    totalRecords={props.data.leaderboards.length}
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
 
export const getServerSideProps: GetServerSideProps<LeaderboardProps> = async(context) => {
    const { type } = context.query;

    try {
        const apiRes = await DoGet('/api/Leaderboards/GetLeaderboard/' + type);

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

export default Leaderboards;