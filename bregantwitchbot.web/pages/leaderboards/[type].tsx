import { DoGet } from "@/helpers/webFetchHelper";
import { GetServerSideProps } from "next";
import { Text } from '@mantine/core';

interface LeaderboardProps {
    data:  GetLeaderboardDto | null;
    error: string | null;
}

interface GetLeaderboardDto {
    leaderboardName: string;
    leaderboards:    LeaderboardData[] | null;
}

interface LeaderboardData {
    position: number;
    username: string;
    amount:   string;
}

const Leaderboards = (props: LeaderboardProps) => {
    return ( 
        <>
            {props.error && 
                <Text>{props.error}</Text>
            }
            
        </>
     );
}
 
export const getServerSideProps: GetServerSideProps<LeaderboardProps> = async(context) => {
    const { consoleId } = context.query;

    try {
        const apiRes = await DoGet('/api/leaderboard/GetLeaderboard' + consoleId);

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