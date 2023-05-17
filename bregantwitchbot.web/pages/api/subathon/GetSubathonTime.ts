import { DoBackendGet } from '@/helpers/backendFetchHelper';
import type { NextApiRequest, NextApiResponse } from 'next'

const handler = async (req: NextApiRequest, res: NextApiResponse<string>) => {
    try {
        const apiRes = await DoBackendGet('api/Subathon/GetSubathonTimeLeft');

        if(!apiRes.ok){
            res.status(apiRes.status).json('Oh no there has been an error getting the time - ' + apiRes.status);
            return;
        }
    
        res.status(200).json(await apiRes.json());
    } catch (error) {
        res.status(500).json('Oh no there has been an error getting the time - 500 error');
    }
}

export default handler;